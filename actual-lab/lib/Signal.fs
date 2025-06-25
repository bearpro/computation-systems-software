module Signal
    #nowarn "25"
    open System.Text

    type Signal = { levels: int list; relativeRate: float }

    let encode (input: string) : int list =
        Encoding.RegisterProvider CodePagesEncodingProvider.Instance
        let bytes = Encoding.GetEncoding(1251).GetBytes input
        bytes
        |> Array.toList
        |> List.collect (fun b ->
            [7..-1..0] |> List.map (fun i -> (int b >>> i) &&& 1)
        )

    let oversample (signal: Signal) (factor: int) : (float * int) list = 
        let sampleRate = signal.relativeRate * float factor
        let levels = signal.levels
        let oversampledLevels =
            levels
            |> List.collect (fun level ->
                [0..factor-1] |> List.map (fun _ -> level))
        oversampledLevels
        |> List.mapi (fun i level -> (float i / sampleRate, level))

    module Formatting =
        /// Converts a list of bits (0s and 1s) to a string representation
        let bitsToString (bits: int list) =
            let potentialStr = function
            | -2 -> "_"   // lowest
            | -1 -> "-"   // low
            |  0 -> "="   // mid
            |  1 -> "+"   // high
            |  2 -> "^"   // highest
            |  _ -> "?"   // out-of-range

            // 1) Pad at front with zeros so total bits % 8 = 0
            let paddedBits =
                let rem = List.length bits % 8
                if rem = 0 then bits
                else List.init (8 - rem) (fun _ -> 0) @ bits

            let bytes =
                paddedBits
                |> List.chunkBySize 8

            let binStrSpaced =
                bytes
                |> List.map (fun byteBits ->
                    byteBits
                    |> List.map potentialStr
                    |> String.concat "")
                |> String.concat " "

            binStrSpaced

        let printBitSeq (message: int list) =
            // 1) Pad at front with zeros so total bits % 8 = 0
            let paddedBits =
                let rem = List.length message % 8
                if rem = 0 then message
                else List.init (8 - rem) (fun _ -> 0) @ message

            // 2) Chunk into bytes
            let bytes =
                paddedBits
                |> List.chunkBySize 8

            // 3) Build hex string (no separators)
            let hexStr =
                bytes
                |> List.map (fun byteBits ->
                    let v = byteBits |> List.fold (fun acc b -> acc * 2 + b) 0
                    sprintf "%02X" v)
                |> String.concat ""

            // 4) Build spaced binary string
            let binStrSpaced =
                bytes
                |> List.map (fun byteBits ->
                    byteBits
                    |> List.map string
                    |> String.concat "")
                |> String.concat " "

            // 5) Lengths
            let bitsLen  = List.length message
            let bytesLen = (bitsLen + 7) / 8

            // Print
            printfn "hex: %s" hexStr
            printfn "bin: %s" binStrSpaced
            printfn "len: %d bytes (%d bits)" bytesLen bitsLen

        let formatFreq (hz: float) =
            if hz >= 1e9 then sprintf "%.2f GHz" (hz / 1e9)
            elif hz >= 1e6 then sprintf "%.2f MHz" (hz / 1e6)
            elif hz >= 1e3 then sprintf "%.2f kHz" (hz / 1e3)
            else sprintf "%.2f Hz" hz

    module PhysicalEncoding =
        let encodeNRZ bits =
            let levels = bits |> List.map (fun b -> if b=1 then 1 else 0)
            { levels = levels; relativeRate = 1.0 }

        let encodeManchester bits =
            let levels = bits |> List.collect (fun b -> if b=1 then [1;0] else [0;1])
            { levels = levels; relativeRate = 2.0 }

        let encodeAMI bits =
            let rec go bits last =
                match bits with
                | [] -> []
                | 0::tl -> 0 :: go tl last
                | 1::tl ->
                    let level = if last=1 then -1 else 1
                    level :: go tl level
            let levels = go bits 0
            { levels = levels; relativeRate = 1.0 }

        let encodeMLT3 bits =
            let states = [|0; 1; 0; -1|]
            let rec go bits idx acc =
                match bits with
                | [] -> List.rev acc
                | 1::tl ->
                    let nextIdx = (idx + 1) % states.Length
                    go tl nextIdx (states.[nextIdx] :: acc)
                | 0::tl ->
                    go tl idx (states.[idx] :: acc)
            let levels = go bits 0 []
            { levels = levels; relativeRate = 1.0 }

        let encodePAM5 bits =
            let pairToLevel = function
                | [0;0] -> -2
                | [0;1] -> -1
                | [1;0] -> 1
                | [1;1] -> 2
                | _ -> 0
            let levels =
                bits
                |> List.chunkBySize 2
                |> List.map pairToLevel
            { levels = levels; relativeRate = 0.5 }

    module LogicalEncoding =
        let encode4b5b (bits: int list) : int list =
            let table =
                Map.ofList [
                    [0;0;0;0], [1;1;1;1;0]
                    [0;0;0;1], [0;1;0;0;1]
                    [0;0;1;0], [1;0;1;0;0]
                    [0;0;1;1], [1;0;1;0;1]
                    [0;1;0;0], [0;1;0;1;0]
                    [0;1;0;1], [0;1;0;1;1]
                    [0;1;1;0], [0;1;1;1;0]
                    [0;1;1;1], [0;1;1;1;1]
                    [1;0;0;0], [1;0;0;1;0]
                    [1;0;0;1], [1;0;0;1;1]
                    [1;0;1;0], [1;0;1;1;0]
                    [1;0;1;1], [1;0;1;1;1]
                    [1;1;0;0], [1;1;0;1;0]
                    [1;1;0;1], [1;1;0;1;1]
                    [1;1;1;0], [1;1;1;0;0]
                    [1;1;1;1], [1;1;1;0;1]
                ]

            bits
            |> List.chunkBySize 4
            |> List.collect (fun nibble ->
                match table.TryFind nibble with
                | Some code -> code
                | None ->
                    failwithf "encode4b5b: неверная 4-битная группа %A" nibble
            )
        
        let scramble (taps: int list) (bits: int list) : int list =
            let maxTap = List.max taps
            let mutable buf = List.replicate (max maxTap bits.Length) 0

            for i in 0 .. bits.Length - 1 do
                let v =
                    taps
                    |> List.fold (fun acc t ->
                        let x = if i >= t then bits.[i - t] else 0
                        acc ^^^ x
                    ) bits.[i]
                buf <- buf |> List.mapi (fun j x -> if j = i then v else x)

            buf.[0 .. bits.Length - 1]

        let scrambleI35 = scramble [3; 5]


    module FrequencyAnalysis =
        type Result = {
            UpperFrequencyBound: float
            LowerFrequencyBound: float
            AverageFrequency: float
            SpectralWidth: float
            EffectiveWidth: float
        }

        let analyze (datarate: int) (signal: Signal) : Result =
            // Effective sample rate (samples per second)
            let sampleRate = float datarate * signal.relativeRate
            let baseFreq = sampleRate / 2.0

            // Detect lengths of runs of equal levels
            let runLengths =
                match signal.levels with
                | [] -> []
                | hd :: tl ->
                    (([], hd, 1), tl)
                    ||> List.fold (fun (acc, prev, count) lvl ->
                        if lvl = prev then (acc, prev, count + 1)
                        else (count :: acc, lvl, 1))
                    |> fun (acc, _, count) -> List.rev (count :: acc)

            match runLengths with
            | [] ->
                { LowerFrequencyBound = 0.0
                  UpperFrequencyBound = 0.0
                  AverageFrequency    = 0.0
                  SpectralWidth       = 0.0
                  EffectiveWidth      = 0.0 }
            | _ ->
                let maxRun = List.max runLengths |> float
                let runCount = List.length runLengths |> float
                let bitCount = List.sum runLengths |> float

                let upper = baseFreq
                let width = baseFreq - (baseFreq / maxRun)
                let average = baseFreq * (runCount / bitCount)
                let effectiveWidth = (7.0 - (1.0 / maxRun)) * baseFreq
                let lower = baseFreq / maxRun

                { LowerFrequencyBound = lower
                  UpperFrequencyBound = upper
                  AverageFrequency    = average
                  SpectralWidth       = width
                  EffectiveWidth      = effectiveWidth }

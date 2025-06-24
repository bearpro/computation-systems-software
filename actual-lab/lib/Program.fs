module Signal
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
                | [b]    -> if b=1 then 1 else -2
                | _ -> 0
            let levels =
                bits
                |> List.chunkBySize 2
                |> List.map pairToLevel
            { levels = levels; relativeRate = 0.5 }

    module FrequencyAnalysis =
        type Result = {
            UpperFrequencyBound: float
            LowerFrequencyBound: float
            AverageFrequency: float
            SpectralWidth: float
        }

        let analyze (bitrate: int) (signal: Signal) : Result =
            // 1. Effective sample rate (samples per second)
            let sampleRate = float bitrate * signal.relativeRate

            // 2. Collect the indices where the level changes
            let transitions =
                signal.levels
                |> List.mapi (fun idx lvl -> idx, lvl)
                |> List.windowed 2
                |> List.choose (fun pair ->
                    match pair with
                    | [ i1, l1; i2, l2 ] when l1 <> l2 -> Some i2
                    | _ -> None)

            match transitions with
            | [] | _ when List.length transitions < 2 ->
                // Not enough transitions to compute a frequency
                { LowerFrequencyBound = 0.0
                  UpperFrequencyBound = 0.0
                  AverageFrequency    = 0.0
                  SpectralWidth  = 0.0 }
            | ts ->
                // 3. Compute instantaneous frequencies
                let freqs =
                    ts
                    |> List.pairwise
                    |> List.map (fun (iPrev, iNext) ->
                        sampleRate / float (iNext - iPrev))

                // 4. Compute statistics
                let minF = List.min freqs
                let maxF = List.max freqs
                let avgF = List.average freqs
                let variance =
                    freqs
                    |> List.map (fun f -> (f - avgF) ** 2.0)
                    |> List.average

                // 5. Return the result
                { LowerFrequencyBound = minF
                  UpperFrequencyBound = maxF
                  AverageFrequency    = avgF
                  SpectralWidth  = minF - maxF }

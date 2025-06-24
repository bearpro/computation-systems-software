module Tests

open System
open Xunit


[<Fact>]
let ``Message encoded correctly`` () =
    let expected: list<int> = [
        // 11010100
        1;1;0;1;0;1;0;0;
        // 00101110
        0;0;1;0;1;1;1;0;
        // 11001000
        1;1;0;0;1;0;0;0;
        // 00101110
        0;0;1;0;1;1;1;0;
        // 11001110
        1;1;0;0;1;1;1;0;
        // 00101110
        0;0;1;0;1;1;1;0
    ]
    let actual = Signal.encode "Ф.И.О."
    Assert.Equal<int>(expected, actual)

open Signal

[<Fact>]
let ``NRZ frequency analysis correct`` () =
    let bits = [0; 1; 0; 1; 0; 1; 0; 0; 0; 0; 1; 1; 1; 1; 0; 1; 1; 0; 0; ]
    let signal = PhysicalEncoding.encodeNRZ bits
    let expected: FrequencyAnalysis.Result = {
        UpperFrequencyBound = 500. * 10. ** 3
        LowerFrequencyBound = 375. * 10. ** 3
        AverageFrequency = 290. * 10. ** 3
        SpectralWidth = 3.375 * 10. ** 6
    }
    let bitrate = int (2. * 10. ** 6.) // 1 mbit/s
    let actual = FrequencyAnalysis.analyze bitrate signal
    printfn "Expected: %A" expected
    printfn "Actual: %A" actual
    Assert.Equal(expected.UpperFrequencyBound, actual.UpperFrequencyBound, 2)

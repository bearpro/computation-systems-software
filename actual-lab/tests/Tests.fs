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
        UpperFrequencyBound = 500. * 10. ** 3 // 500 kHz
        LowerFrequencyBound = 375. * 10. ** 3 // 375 kHz
        AverageFrequency = 290. * 10. ** 3 // 290 kHz
        SpectralWidth = 3.375 * 10. ** 6 // 3.375 MHz
        EffectiveWidth = 375. * 10. ** 3 // 375 kHz
    }
    let datarate = int (1. * 10. ** 6.) // 1 mbit/s
    let actual = FrequencyAnalysis.analyze datarate signal
    // printfn "Expected: %A" expected
    // printfn "Actual: %A" actual
    Assert.Equal(expected.UpperFrequencyBound, actual.UpperFrequencyBound, 2)


[<Fact>]
let ``Oversempling should provide correct bit offsets with relativeRate 1`` () =
    let input : Signal = { levels = [0; 1]; relativeRate = 1}
    let expected = [
        0.0, 0
        0.5, 0
        1.0, 1
        1.5, 1
    ]
    let actual = Signal.oversample input 2
    Assert.Equal<float * int>(expected, actual)

[<Fact>]
let ``Oversempling should provide correct bit offsets with relativeRate 2`` () =
    let input : Signal = { levels = [0; 0; 1; 1]; relativeRate = 2}
    let expected = [
        0.0, 0
        0.25, 0
        0.5, 0
        0.75, 0
        1.0, 1
        1.25, 1
        1.5, 1
        1.75, 1
    ]
    let actual = Signal.oversample input 2
    Assert.Equal<float * int>(expected, actual)
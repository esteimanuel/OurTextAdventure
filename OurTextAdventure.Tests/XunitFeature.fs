module OurTextAdventure.Tests.XunitFeature

open TickSpec
open Xunit

let source = AssemblyStepDefinitionsSource(System.Reflection.Assembly.GetExecutingAssembly())
let scenarios resourceName = source.ScenariosFromEmbeddedResource resourceName |> MemberData.ofScenarios

[<Theory; MemberData("scenarios", "OurTextAdventure.Tests.PlayerDetails.feature")>]
let PlayerDetails (scenario : Scenario) = scenario.Action.Invoke()
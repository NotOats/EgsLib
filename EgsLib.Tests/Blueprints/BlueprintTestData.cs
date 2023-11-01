using EgsLib.Blueprints;
using System.Collections;
using System.Diagnostics;

namespace EgsLib.Tests.Blueprints
{
    public class BlueprintTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            /*
             * From blueprint factory page in game
             * 
             * Constructor 1.5: HV, Lv 5 Unlock, Class 1, WxHxD 3,2,3, BlockCnt 16, DeviceCnt 5, TriangleCnt 68
             *                  53 Copper, 50 Iron, 37 Silicon, 28 Carbon
             * 
             * Fridge: HV, Lv 3 Unlock, Class 1, WxHxD 3x2x2, BlockCnt 5, DeviceCnt 5, TriangleCnt none?
             *         46 Carbon, 33 Copper, 25 Iron, 23 Silicon
             * 
             * Lg Constructor: Base, Lv 5 Unlock, Class 1, WxHxD 3x2x3, BlockCnt 7, DeviceCnt 7, TriangleCnt none?
             *                 Carbon 152, Copper 146, Iron 105, Silicon 96
             * 
             * Cargo Box - M: CV, Lv 10 Unlock, Class 1, Combat (Atk/Def) 0/62, WxHxD 7x6x9, BlockCnt 247, DeviceCnt 16, TriangleCnt 3,028
             *                Iron 1593, Carbon 1300, Copper 1118, Silicon 637, Neodymium 464, Titanium 300, Cobalt 217
             * 
             * Pocket SV: SV, Lv 5 Unlock, Class 1, Combat (Atk/Def) 0/13, WxHxD 3x3x9, BlockCnt 17, DeviceCnt 13, TriangleCnt 28
             *            Carbon 170, Copper 110, Iron 90, Silicon 82
             */

            yield return new object[]
                {
                    new BlueprintDetails(
                        BlueprintType.HoverVessel,
                        @"Resources\Blueprints\[Basics] Constructor 1.5\[Basics] Constructor 1.5.epb")
                    {
                        Size = new Vector3<int>(3, 2, 3),
                        BlockCount = 16,
                        DeviceCount = 5,
                        TriangleCount = 68,
                        SizeClass = 1
                    }
                };

            yield return new object[]
                {
                    new BlueprintDetails(
                        BlueprintType.HoverVessel,
                        @"Resources\Blueprints\[Basics] Fridge\[Basics] Fridge.epb")
                    {
                        Size = new Vector3<int>(3, 2, 2),
                        BlockCount = 5,
                        DeviceCount = 5,
                        SizeClass = 1
                    }
                };

            yield return new object[]
                {
                    new BlueprintDetails(
                        BlueprintType.Base,
                        @"Resources\Blueprints\[Basics] Lg Constructor\[Basics] Lg Constructor.epb")
                    {
                        Size = new Vector3<int>(3, 2, 3),
                        BlockCount = 7,
                        DeviceCount = 7,
                        SizeClass = 1
                    }
                };

            yield return new object[]
                {
                    new BlueprintDetails(
                        BlueprintType.CapticalVessel,
                        @"Resources\Blueprints\Cargo Box - M\Cargo Box - M.epb")
                    {
                        Size = new Vector3<int>(7, 6, 9),
                        BlockCount = 247,
                        DeviceCount = 16,
                        TriangleCount = 3028,
                        SizeClass = 1
                    }
                };

            yield return new object[]
                {
                    new BlueprintDetails(
                        BlueprintType.SmallVessel,
                        @"Resources\Blueprints\Pocket SV\Pocket SV.epb")
                    {
                        Size          = new Vector3<int>(3, 3, 9),
                        BlockCount    = 17,
                        DeviceCount   = 13,
                        TriangleCount = 28,
                        SizeClass = 1
                    }
                };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class BlueprintDetails
    {
        private static string ProgramFolder =>
            Path.GetDirectoryName(Process.GetCurrentProcess()?.MainModule?.FileName)
            ?? throw new Exception("Failed to find current program folder");

        public string File { get; }
        public BlueprintType Type { get; }
        public Vector3<int> Size { get; init; }
        public int BlockCount { get; init; }
        public int DeviceCount { get; init; }
        public int TriangleCount { get; init; }
        public int SizeClass { get; init; }

        public BlueprintDetails(BlueprintType type, string file)
        {
            Type = type;
            File = Path.Combine(ProgramFolder, file);
        }
    }
}

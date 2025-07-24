using EgsLib.Blueprints;
using EgsLib.Extensions;
using System.Collections.Generic;
using System.IO;
using Xunit;
using System;

namespace EgsLib.Tests.Blueprints
{
    public class MutableBlueprintTest
    {
        // Hull block IDs for different blueprint types
        private const int SV_HULL_BLOCK_ID = 381; // Small Vessel / Hover Vessel
        private const int CV_HULL_BLOCK_ID = 403; // Capital Vessel / Base

        // Proper path to the Simple Cube blueprint (same as BlueprintSerializationTest)
        private const string SIMPLE_CUBE_PATH = @"Resources\Blueprints\Simple Cube\Simple Cube.epb";

        [Fact]
        public void CanMutateBlueprintDirectly()
        {
            // Arrange: Load the simple cube blueprint (3x3x3 CV with core at 1,1,1)
            var blueprint = new Blueprint(SIMPLE_CUBE_PATH);

            // Act: Add hull blocks outside the existing 3x3x3 cube
            var newHullPositions = new List<Vector3<int>>
            {
                new Vector3<int>(3, 1, 1), // Right of cube
                new Vector3<int>(1, 3, 1), // Above cube  
                new Vector3<int>(1, 1, 3), // In front of cube
            };

            // Add hull blocks using AddBlock with CV hull block ID (since it's a CV blueprint)
            foreach (var position in newHullPositions)
            {
                blueprint.AddBlock(position, CV_HULL_BLOCK_ID);
            }

            // Update metadata
            blueprint.SetDisplayName("My Modified Ship");

            // Assert: Verify the changes
            Assert.True(blueprint.Header.GetProperty<string>(PropertyName.DisplayName, out var displayName));
            Assert.Equal("My Modified Ship", displayName);

            // Verify blocks were added (only check positions with non-negative coordinates)
            foreach (var position in newHullPositions)
            {
                var block = blueprint.GetBlock(position);
                Assert.Equal(CV_HULL_BLOCK_ID, block.BlockId);
                Assert.False(block.IsEmpty);
            }
        }

        [Fact]
        public void CanAddColoredHullBlocks()
        {
            // Arrange: Load the simple cube blueprint
            var blueprint = new Blueprint(SIMPLE_CUBE_PATH);

            // Act: Add colored hull blocks outside the existing cube
            blueprint.AddBlock(new Vector3<int>(3, 1, 1), CV_HULL_BLOCK_ID, color: 0xFF0000); // Red
            blueprint.AddBlock(new Vector3<int>(1, 3, 1), CV_HULL_BLOCK_ID, color: 0x00FF00); // Green

            // Assert: Verify colors
            var redBlock = blueprint.GetBlock(new Vector3<int>(3, 1, 1));
            Assert.Equal(0xFF0000, redBlock.Color);

            var greenBlock = blueprint.GetBlock(new Vector3<int>(1, 3, 1));
            Assert.Equal(0x00FF00, greenBlock.Color);
        }

        [Fact]
        public void CanRemoveBlocks()
        {
            // Arrange: Load the simple cube blueprint
            var blueprint = new Blueprint(SIMPLE_CUBE_PATH);

            // Use a position that should have an existing block (corner of the 3x3x3 cube)
            var position = new Vector3<int>(0, 0, 0);
            
            // Verify block exists initially
            var existingBlock = blueprint.GetBlock(position);
            Assert.False(existingBlock.IsEmpty, "Expected existing block at (0,0,0)");

            // Act: Remove the block
            blueprint.RemoveBlock(position);

            // Assert: Block should be empty now
            var removedBlock = blueprint.GetBlock(position);
            Assert.True(removedBlock.IsEmpty);
        }

        [Fact]
        public void CanReplaceExistingBlocks()
        {
            // Arrange: Load the simple cube blueprint
            var blueprint = new Blueprint(SIMPLE_CUBE_PATH);

            // Use a position that should have an existing block
            var position = new Vector3<int>(0, 1, 1);
            
            // Verify block exists initially
            var existingBlock = blueprint.GetBlock(position);
            Assert.False(existingBlock.IsEmpty, "Expected existing block at (0,1,1)");
            
            // Act: Replace the block with a colored one
            blueprint.AddBlock(position, CV_HULL_BLOCK_ID, color: 0xFF0000);

            // Assert: Block should still exist but now be red
            var replacedBlock = blueprint.GetBlock(position);
            Assert.False(replacedBlock.IsEmpty);
            Assert.Equal(CV_HULL_BLOCK_ID, replacedBlock.BlockId);
            Assert.Equal(0xFF0000, replacedBlock.Color);
        }

        [Fact]
        public void CanSaveModifiedBlueprint()
        {
            // Arrange: Load the simple cube blueprint
            var blueprint = new Blueprint(SIMPLE_CUBE_PATH);
            var tempPath = Path.GetTempFileName();
            var outputPath = Path.ChangeExtension(tempPath, ".epb");

            try
            {
                // Act: Modify blueprint by adding blocks outside existing cube
                blueprint.SetDisplayName("Test Save Blueprint");
                blueprint.AddBlock(new Vector3<int>(3, 1, 1), CV_HULL_BLOCK_ID);
                blueprint.AddBlock(new Vector3<int>(1, 3, 1), CV_HULL_BLOCK_ID);

                // Save using the blueprint's SaveTo method
                blueprint.SaveTo(outputPath);

                // Assert: File should exist and be readable
                Assert.True(File.Exists(outputPath));

                // Verify we can load the saved blueprint
                var loadedBlueprint = new Blueprint(outputPath);
                Assert.True(loadedBlueprint.Header.GetProperty<string>(PropertyName.DisplayName, out var displayName));
                Assert.Equal("Test Save Blueprint", displayName);

                // Verify blocks are preserved
                var block1 = loadedBlueprint.GetBlock(new Vector3<int>(3, 1, 1));
                var block2 = loadedBlueprint.GetBlock(new Vector3<int>(1, 3, 1));
                Assert.Equal(CV_HULL_BLOCK_ID, block1.BlockId);
                Assert.Equal(CV_HULL_BLOCK_ID, block2.BlockId);

                // Output diagnostic info
                Console.WriteLine($"📊 Original file size: {new FileInfo(SIMPLE_CUBE_PATH).Length} bytes");
                Console.WriteLine($"📊 Modified file size: {new FileInfo(outputPath).Length} bytes");
                Console.WriteLine($"📏 Size difference: {new FileInfo(outputPath).Length - new FileInfo(SIMPLE_CUBE_PATH).Length} bytes");
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempPath)) File.Delete(tempPath);
                if (File.Exists(outputPath)) File.Delete(outputPath);
            }
        }

        [Fact]
        public void ModifiedBlueprint_ShouldPreserveOriginalStructure()
        {
            // Arrange: Load the simple cube blueprint
            var originalBlueprint = new Blueprint(SIMPLE_CUBE_PATH);
            var tempPath = Path.GetTempFileName();
            var outputPath = Path.ChangeExtension(tempPath, ".epb");

            try
            {
                // Act: Make modifications
                var modifiedBlueprint = new Blueprint(SIMPLE_CUBE_PATH);
                modifiedBlueprint.SetDisplayName("Structure Preservation Test");
                modifiedBlueprint.SetCreatorName("EgsLib Test");
                modifiedBlueprint.AddBlock(new Vector3<int>(3, 1, 1), CV_HULL_BLOCK_ID); // This will expand the blueprint

                // Save and reload
                modifiedBlueprint.SaveTo(outputPath);
                var reloadedBlueprint = new Blueprint(outputPath);

                // Assert: Critical structure should be preserved
                Assert.Equal(originalBlueprint.Header.Version, reloadedBlueprint.Header.Version);
                Assert.Equal(originalBlueprint.Header.BlueprintType, reloadedBlueprint.Header.BlueprintType);
                
                // Size should be expanded since we added a block outside original bounds
                Assert.True(reloadedBlueprint.BlockData.Size.X >= originalBlueprint.BlockData.Size.X);
                Assert.True(reloadedBlueprint.BlockData.Size.Y >= originalBlueprint.BlockData.Size.Y);
                Assert.True(reloadedBlueprint.BlockData.Size.Z >= originalBlueprint.BlockData.Size.Z);
                
                // BlocksSize should be updated accordingly
                var expectedBlocksSize = reloadedBlueprint.BlockData.Size.X * 
                                       reloadedBlueprint.BlockData.Size.Y * 
                                       reloadedBlueprint.BlockData.Size.Z;
                Assert.Equal(expectedBlocksSize, reloadedBlueprint.BlockData.BlocksSize);

                // Verify the modification was preserved
                Assert.True(reloadedBlueprint.Header.GetProperty<string>(PropertyName.DisplayName, out var displayName));
                Assert.Equal("Structure Preservation Test", displayName);

                var addedBlock = reloadedBlueprint.GetBlock(new Vector3<int>(3, 1, 1));
                Assert.Equal(CV_HULL_BLOCK_ID, addedBlock.BlockId);
                Assert.False(addedBlock.IsEmpty);

                // Count total blocks to ensure we added one
                var originalBlockCount = CountNonEmptyBlocks(originalBlueprint);
                var modifiedBlockCount = CountNonEmptyBlocks(reloadedBlueprint);
                Assert.Equal(originalBlockCount + 1, modifiedBlockCount);

                Console.WriteLine($"✅ Structure preserved successfully with expansion");
                Console.WriteLine($"📊 Original size: {originalBlueprint.BlockData.Size}");
                Console.WriteLine($"📊 Expanded size: {reloadedBlueprint.BlockData.Size}");
                Console.WriteLine($"📊 Original blocks: {originalBlockCount}");
                Console.WriteLine($"📊 Modified blocks: {modifiedBlockCount}");
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempPath)) File.Delete(tempPath);
                if (File.Exists(outputPath)) File.Delete(outputPath);
            }
        }

        [Fact]
        public void CreateModifiedBlueprintForGameTesting()
        {
            // Arrange: Load the simple cube blueprint (3x3x3 CV)
            var blueprint = new Blueprint(SIMPLE_CUBE_PATH);
            var outputDirectory = @"..\..\..\..\TestOutput";
            var outputPath = Path.Combine(outputDirectory, "ExtendedCV_MutableTest.epb");

            // Ensure output directory exists
            Directory.CreateDirectory(outputDirectory);

            // Act: Create extensions to the cube
            blueprint.SetDisplayName("Extended CV (Mutable Test)");
            blueprint.SetCreatorName("EgsLib Mutable Test");

            // Add simple extensions (same as working test)
            var extensions = new List<Vector3<int>>
            {
                new Vector3<int>(3, 1, 1), // Right of cube
                new Vector3<int>(1, 3, 1), // Above cube  
                new Vector3<int>(1, 1, 3), // In front of cube
                new Vector3<int>(0, 3, 1), // Another extension
                new Vector3<int>(3, 0, 1)  // Another extension
            };

            // Add all extension blocks using CV hull block ID
            foreach (var position in extensions)
            {
                blueprint.AddBlock(position, CV_HULL_BLOCK_ID);
            }

            // Save the modified blueprint
            blueprint.SaveTo(outputPath);

            // Verify the saved file can be loaded
            var verificationBlueprint = new Blueprint(outputPath);
            
            // Assert basic structure is preserved
            Assert.NotNull(verificationBlueprint);
            Assert.True(verificationBlueprint.Header.GetProperty<string>(PropertyName.DisplayName, out var displayName));
            Assert.Equal("Extended CV (Mutable Test)", displayName);

            // Verify extensions were added
            foreach (var position in extensions)
            {
                var block = verificationBlueprint.GetBlock(position);
                Assert.Equal(CV_HULL_BLOCK_ID, block.BlockId);
                Assert.False(block.IsEmpty);
            }

            // Output info for user
            var fileInfo = new FileInfo(outputPath);
            var absolutePath = Path.GetFullPath(outputPath);
            
            Console.WriteLine($"📁 Extended CV created: {absolutePath}");
            Console.WriteLine($"📊 Size: {blueprint.BlockData.Size}");
            Console.WriteLine($"📦 File size: {fileInfo.Length} bytes");
            Console.WriteLine($"🚀 Extended 3x3x3 cube with {extensions.Count} additional hull blocks");
            Console.WriteLine($"🔧 Blueprint Version: {blueprint.Header.Version}");
            Console.WriteLine($"🎮 Ready for in-game testing!");

            // Assert for test validation
            Assert.True(File.Exists(outputPath));
            Assert.True(fileInfo.Length > 0);
        }

        [Fact]
        public void CompareOriginalVsModified_FileSizes()
        {
            // Arrange: Load original blueprint
            var originalBlueprint = new Blueprint(SIMPLE_CUBE_PATH);
            var tempPath = Path.GetTempFileName();
            var outputPath = Path.ChangeExtension(tempPath, ".epb");

            try
            {
                // Act: Create a modified version
                var modifiedBlueprint = new Blueprint(SIMPLE_CUBE_PATH);
                modifiedBlueprint.SetDisplayName("Size Comparison Test");
                
                // Add several blocks
                var newBlocks = new List<Vector3<int>>
                {
                    new Vector3<int>(3, 1, 1),
                    new Vector3<int>(1, 3, 1),
                    new Vector3<int>(1, 1, 3),
                    new Vector3<int>(4, 1, 1),
                    new Vector3<int>(1, 4, 1)
                };

                foreach (var position in newBlocks)
                {
                    modifiedBlueprint.AddBlock(position, CV_HULL_BLOCK_ID);
                }

                modifiedBlueprint.SaveTo(outputPath);

                // Assert and analyze sizes
                var originalFileInfo = new FileInfo(SIMPLE_CUBE_PATH);
                var modifiedFileInfo = new FileInfo(outputPath);

                Assert.True(modifiedFileInfo.Length > 0);
                
                Console.WriteLine($"🔍 FILE SIZE COMPARISON:");
                Console.WriteLine($"   Original: {originalFileInfo.Length} bytes");
                Console.WriteLine($"   Modified: {modifiedFileInfo.Length} bytes");
                Console.WriteLine($"   Difference: {modifiedFileInfo.Length - originalFileInfo.Length} bytes");
                Console.WriteLine($"   Added blocks: {newBlocks.Count}");
                Console.WriteLine($"   Bytes per added block: {(modifiedFileInfo.Length - originalFileInfo.Length) / (float)newBlocks.Count:F1}");
                
                // Verify the modified blueprint loads correctly
                var reloadedBlueprint = new Blueprint(outputPath);
                Assert.NotNull(reloadedBlueprint);
                
                var originalBlockCount = CountNonEmptyBlocks(originalBlueprint);
                var modifiedBlockCount = CountNonEmptyBlocks(reloadedBlueprint);
                
                Console.WriteLine($"   Original block count: {originalBlockCount}");
                Console.WriteLine($"   Modified block count: {modifiedBlockCount}");
                Console.WriteLine($"   Block count difference: {modifiedBlockCount - originalBlockCount}");
                
                Assert.Equal(originalBlockCount + newBlocks.Count, modifiedBlockCount);
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempPath)) File.Delete(tempPath);
                if (File.Exists(outputPath)) File.Delete(outputPath);
            }
        }

        [Fact]
        public void DiagnoseBoundaryExpansion()
        {
            // Arrange: Load the simple cube blueprint (3x3x3 CV)
            var blueprint = new Blueprint(SIMPLE_CUBE_PATH);
            
            Console.WriteLine($"📊 Original blueprint size: {blueprint.BlockData.Size}");
            Console.WriteLine($"📊 Original blocks size: {blueprint.BlockData.BlocksSize}");
            
            // Count original non-empty blocks
            var originalBlocks = CountNonEmptyBlocks(blueprint);
            Console.WriteLine($"📊 Original non-empty blocks: {originalBlocks}");

            // Act: Try to add a block far away
            var farPosition = new Vector3<int>(5, 1, 1);
            Console.WriteLine($"🎯 Adding block at position {farPosition}");
            
            blueprint.AddBlock(farPosition, CV_HULL_BLOCK_ID);
            
            Console.WriteLine($"📊 After expansion - size: {blueprint.BlockData.Size}");
            Console.WriteLine($"📊 After expansion - blocks size: {blueprint.BlockData.BlocksSize}");
            
            // Check if the block was added correctly
            var addedBlock = blueprint.GetBlock(farPosition);
            Console.WriteLine($"🔍 Block at {farPosition}: ID={addedBlock.BlockId}, Empty={addedBlock.IsEmpty}, Data={addedBlock.Data}");
            
            // Count blocks after addition
            var newBlocks = CountNonEmptyBlocks(blueprint);
            Console.WriteLine($"📊 After addition - non-empty blocks: {newBlocks}");
            Console.WriteLine($"📊 Expected increase: {newBlocks - originalBlocks} (should be 1)");
            
            // Test a few existing blocks to make sure they weren't corrupted
            var testPositions = new[]
            {
                new Vector3<int>(0, 0, 0),
                new Vector3<int>(1, 1, 1), // Core position
                new Vector3<int>(2, 2, 2)
            };
            
            Console.WriteLine("🔍 Checking existing blocks after expansion:");
            foreach (var pos in testPositions)
            {
                var block = blueprint.GetBlock(pos);
                Console.WriteLine($"   Position {pos}: ID={block.BlockId}, Empty={block.IsEmpty}");
            }
            
            // Now test save/load cycle
            var tempPath = Path.GetTempFileName();
            var outputPath = Path.ChangeExtension(tempPath, ".epb");
            
            try
            {
                blueprint.SaveTo(outputPath);
                var reloadedBlueprint = new Blueprint(outputPath);
                
                Console.WriteLine($"📊 After reload - size: {reloadedBlueprint.BlockData.Size}");  
                var reloadedBlock = reloadedBlueprint.GetBlock(farPosition);
                Console.WriteLine($"🔍 Reloaded block at {farPosition}: ID={reloadedBlock.BlockId}, Empty={reloadedBlock.IsEmpty}");
                
                var reloadedBlocks = CountNonEmptyBlocks(reloadedBlueprint);
                Console.WriteLine($"📊 After reload - non-empty blocks: {reloadedBlocks}");
                
                // This is the assertion that was failing
                Assert.Equal(CV_HULL_BLOCK_ID, reloadedBlock.BlockId);
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
                if (File.Exists(outputPath)) File.Delete(outputPath);
            }
        }

        [Fact]
        public void DiagnoseMultipleExpansions()
        {
            // Arrange: Load the simple cube blueprint (3x3x3 CV)
            var blueprint = new Blueprint(SIMPLE_CUBE_PATH);
            
            Console.WriteLine($"📊 Original blueprint size: {blueprint.BlockData.Size}");
            var originalBlocks = CountNonEmptyBlocks(blueprint);
            Console.WriteLine($"📊 Original non-empty blocks: {originalBlocks}");

            // Act: Replicate the original failing pattern
            var extensions = new List<Vector3<int>>();

            // Extend right (positive X) - original failing pattern
            for (int x = 3; x <= 5; x++)
            {
                extensions.Add(new Vector3<int>(x, 1, 1));
            }

            // Extend up (positive Y) - original failing pattern  
            for (int y = 3; y <= 5; y++)
            {
                extensions.Add(new Vector3<int>(1, y, 1));
            }

            // Extend forward (positive Z) - original failing pattern
            for (int z = 3; z <= 5; z++)
            {
                extensions.Add(new Vector3<int>(1, 1, z));
            }

            Console.WriteLine($"🎯 Adding {extensions.Count} blocks at distant positions:");
            foreach (var pos in extensions)
            {
                Console.WriteLine($"   Adding block at {pos}");
                blueprint.AddBlock(pos, CV_HULL_BLOCK_ID);
                
                // Check immediately after each addition
                var checkBlock = blueprint.GetBlock(pos);
                Console.WriteLine($"   Immediate check: ID={checkBlock.BlockId}, Empty={checkBlock.IsEmpty}");
                
                Console.WriteLine($"   Blueprint size after adding {pos}: {blueprint.BlockData.Size}");
            }
            
            Console.WriteLine($"📊 Final size: {blueprint.BlockData.Size}");
            var afterAdditionBlocks = CountNonEmptyBlocks(blueprint);
            Console.WriteLine($"📊 After all additions - non-empty blocks: {afterAdditionBlocks}");
            Console.WriteLine($"📊 Expected increase: {afterAdditionBlocks - originalBlocks} (should be {extensions.Count})");
            
            // Check each added block before save
            Console.WriteLine("🔍 Checking all added blocks before save:");
            foreach (var pos in extensions)
            {
                var block = blueprint.GetBlock(pos);
                Console.WriteLine($"   Position {pos}: ID={block.BlockId}, Empty={block.IsEmpty}");
            }
            
            // Test save/load cycle
            var tempPath = Path.GetTempFileName();
            var outputPath = Path.ChangeExtension(tempPath, ".epb");
            
            try
            {
                blueprint.SaveTo(outputPath);
                var reloadedBlueprint = new Blueprint(outputPath);
                
                Console.WriteLine($"📊 After reload - size: {reloadedBlueprint.BlockData.Size}");
                var reloadedBlocks = CountNonEmptyBlocks(reloadedBlueprint);
                Console.WriteLine($"📊 After reload - non-empty blocks: {reloadedBlocks}");
                
                // Check each block after reload
                Console.WriteLine("🔍 Checking all blocks after reload:");
                foreach (var pos in extensions)
                {
                    var block = reloadedBlueprint.GetBlock(pos);
                    Console.WriteLine($"   Position {pos}: ID={block.BlockId}, Empty={block.IsEmpty}");
                    
                    // This is where the original test was failing
                    if (block.BlockId != CV_HULL_BLOCK_ID)
                    {
                        Console.WriteLine($"❌ FAILED: Expected {CV_HULL_BLOCK_ID}, got {block.BlockId} at {pos}");
                    }
                }
                
                // Let's see if any blocks got corrupted by checking a few originals
                Console.WriteLine("🔍 Checking some original blocks:");
                var originalPositions = new[] { 
                    new Vector3<int>(0, 0, 0), 
                    new Vector3<int>(1, 1, 1), 
                    new Vector3<int>(2, 1, 1) 
                };
                foreach (var pos in originalPositions)
                {
                    var block = reloadedBlueprint.GetBlock(pos);
                    Console.WriteLine($"   Position {pos}: ID={block.BlockId}, Empty={block.IsEmpty}");
                }
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
                if (File.Exists(outputPath)) File.Delete(outputPath);
            }
        }

        [Fact]
        public void DiagnoseBlockDisappearance()
        {
            // Arrange: Load the simple cube blueprint (3x3x3 CV)
            var blueprint = new Blueprint(SIMPLE_CUBE_PATH);
            
            Console.WriteLine($"📊 Starting with size: {blueprint.BlockData.Size}");
            
            // Act: Add blocks one by one and check what happens to previous blocks
            var positions = new List<Vector3<int>>
            {
                new Vector3<int>(3, 1, 1), // X expansion: 3x3x3 -> 4x3x3
                new Vector3<int>(4, 1, 1), // X expansion: 4x3x3 -> 5x3x3
                new Vector3<int>(1, 3, 1), // Y expansion: 5x3x3 -> 5x4x3
            };

            var addedBlocks = new List<Vector3<int>>();

            foreach (var pos in positions)
            {
                Console.WriteLine($"\n🎯 Adding block at {pos}");
                Console.WriteLine($"   Size before: {blueprint.BlockData.Size}");
                
                blueprint.AddBlock(pos, CV_HULL_BLOCK_ID);
                addedBlocks.Add(pos);
                
                Console.WriteLine($"   Size after: {blueprint.BlockData.Size}");
                
                // Check all previously added blocks
                Console.WriteLine($"   Checking all {addedBlocks.Count} added blocks:");
                foreach (var checkPos in addedBlocks)
                {
                    var block = blueprint.GetBlock(checkPos);
                    Console.WriteLine($"     {checkPos}: ID={block.BlockId}, Empty={block.IsEmpty}");
                    
                    if (block.IsEmpty)
                    {
                        Console.WriteLine($"❌ BLOCK DISAPPEARED during expansion to {pos}!");
                    }
                }
                
                // Also check a few original blocks
                var originalPos = new Vector3<int>(1, 1, 1);
                var origBlock = blueprint.GetBlock(originalPos);
                Console.WriteLine($"   Original block at {originalPos}: ID={origBlock.BlockId}, Empty={origBlock.IsEmpty}");
                if (origBlock.IsEmpty)
                {
                    Console.WriteLine($"❌ ORIGINAL BLOCK CORRUPTED during expansion to {pos}!");
                }
            }
        }

        /// <summary>
        /// Helper method to count non-empty blocks in a blueprint
        /// </summary>
        private int CountNonEmptyBlocks(Blueprint blueprint)
        {
            int count = 0;
            var blocks = blueprint.BlockData.Blocks;
            for (int i = 0; i < blueprint.BlockData.BlocksSize; i++)
            {
                if (!blocks[i].IsEmpty)
                    count++;
            }
            return count;
        }

        [Fact]
        public void PerformanceTest_BulkOperations_ShouldBeFastWithLazyStatistics()
        {
            // Arrange: Load the simple cube blueprint (3x3x3 CV)
            var blueprint = new Blueprint(SIMPLE_CUBE_PATH);
            
            Console.WriteLine($"🚀 PERFORMANCE TEST: Bulk Block Operations");
            Console.WriteLine($"📊 Starting blueprint size: {blueprint.BlockData.Size}");
            
            // Test adding many blocks (simulating voxelization)
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var blocksToAdd = 100;
            
            // Add blocks in a grid pattern
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    var position = new Vector3<int>(x + 3, y + 3, 1);
                    blueprint.AddBlock(position, 403); // CV hull block
                }
            }
            
            stopwatch.Stop();
            var addTime = stopwatch.ElapsedMilliseconds;
            
            Console.WriteLine($"⏱️  Added {blocksToAdd} blocks in {addTime} ms");
            Console.WriteLine($"📈 Average time per block: {(double)addTime / blocksToAdd:F2} ms");
            
            // Verify that statistics are still dirty (not calculated yet)
            Console.WriteLine($"📊 Final blueprint size: {blueprint.BlockData.Size}");
            
            // Now access statistics to trigger calculation
            stopwatch.Restart();
            var stats = blueprint.Header.Statistics;
            stopwatch.Stop();
            var statsTime = stopwatch.ElapsedMilliseconds;
            
            Console.WriteLine($"📊 Statistics calculation took: {statsTime} ms");
            Console.WriteLine($"🧮 Total blocks: {stats.BlockSolids}");
            
            // Verify performance - bulk operations should be very fast
            Assert.True(addTime < 1000, $"Bulk operations too slow: {addTime} ms for {blocksToAdd} blocks");
            Assert.True(addTime / (double)blocksToAdd < 5, $"Average time per block too high: {(double)addTime / blocksToAdd:F2} ms");
            
            Console.WriteLine($"✅ Performance test passed! Lazy statistics working correctly.");
        }
    }
}
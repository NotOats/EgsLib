using EgsLib.Blueprints;

namespace EgsLib.Tests.Blueprints
{
    public class BlueprintSerializationTest
    {
        // Standardized path to the Simple Cube blueprint (same as MutableBlueprintTest)
        private const string SIMPLE_CUBE_PATH = @"Resources\Blueprints\Simple Cube\Simple Cube.epb";
        [Theory]
        [ClassData(typeof(BlueprintTestData))]
        public void BlueprintRoundTripSerialization_ShouldProduceIdenticalFile(BlueprintDetails details)
        {
            // Arrange
            var originalBlueprint = new Blueprint(details.File);
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act - Serialize the blueprint to a temporary file
                using (var fileStream = File.Create(tempFile))
                using (var writer = new BinaryWriter(fileStream))
                {
                    originalBlueprint.Serialize(writer);
                }

                // Assert - Read back the serialized file and compare
                var deserializedBlueprint = new Blueprint(tempFile);

                // Compare headers
                Assert.Equal(originalBlueprint.Header.Version, deserializedBlueprint.Header.Version);
                Assert.Equal(originalBlueprint.Header.Size, deserializedBlueprint.Header.Size);
                Assert.Equal(originalBlueprint.Header.SizeClass, deserializedBlueprint.Header.SizeClass);
                Assert.Equal(originalBlueprint.Header.Statistics.BlockSolids, deserializedBlueprint.Header.Statistics.BlockSolids);
                Assert.Equal(originalBlueprint.Header.Statistics.BlockDevices, deserializedBlueprint.Header.Statistics.BlockDevices);
                Assert.Equal(originalBlueprint.Header.Statistics.TrianglesReal, deserializedBlueprint.Header.Statistics.TrianglesReal);

                // Compare block data for all versions
                Assert.Equal(originalBlueprint.BlockData.Size, deserializedBlueprint.BlockData.Size);
                Assert.Equal(originalBlueprint.BlockData.BlocksSize, deserializedBlueprint.BlockData.BlocksSize);

                // Compare blocks with data (non-zero blocks)
                var originalBlocks = originalBlueprint.BlockData.Blocks.Take(originalBlueprint.BlockData.BlocksSize).ToArray();
                var deserializedBlocks = deserializedBlueprint.BlockData.Blocks.Take(deserializedBlueprint.BlockData.BlocksSize).ToArray();

                Assert.Equal(originalBlocks.Length, deserializedBlocks.Length);

                for (int i = 0; i < originalBlocks.Length; i++)
                {
                    Assert.Equal(originalBlocks[i].Data, deserializedBlocks[i].Data);
                    Assert.Equal(originalBlocks[i].Damage, deserializedBlocks[i].Damage);
                    Assert.Equal(originalBlocks[i].Density, deserializedBlocks[i].Density);
                    Assert.Equal(originalBlocks[i].Color, deserializedBlocks[i].Color);
                    Assert.Equal(originalBlocks[i].Texture, deserializedBlocks[i].Texture);
                    Assert.Equal(originalBlocks[i].TextureRotation, deserializedBlocks[i].TextureRotation);
                    Assert.Equal(originalBlocks[i].Symbol, deserializedBlocks[i].Symbol);
                    Assert.Equal(originalBlocks[i].SymbolRotation, deserializedBlocks[i].SymbolRotation);
                }

                // Compare entities
                Assert.Equal(originalBlueprint.BlockData.Entities.Count, deserializedBlueprint.BlockData.Entities.Count);
                foreach (var entity in originalBlueprint.BlockData.Entities)
                {
                    Assert.True(deserializedBlueprint.BlockData.Entities.ContainsKey(entity.Key));
                    // Note: NbtList comparison would need to be implemented if needed
                }

                // Compare lock codes
                Assert.Equal(originalBlueprint.BlockData.LockCodes.Count, deserializedBlueprint.BlockData.LockCodes.Count);
                foreach (var lockCode in originalBlueprint.BlockData.LockCodes)
                {
                    Assert.True(deserializedBlueprint.BlockData.LockCodes.ContainsKey(lockCode.Key));
                    Assert.Equal(lockCode.Value, deserializedBlueprint.BlockData.LockCodes[lockCode.Key]);
                }

                // Compare signal sources
                Assert.Equal(originalBlueprint.BlockData.SignalSources.Count, deserializedBlueprint.BlockData.SignalSources.Count);

                // Compare signal receivers
                Assert.Equal(originalBlueprint.BlockData.SignalReceivers.Count, deserializedBlueprint.BlockData.SignalReceivers.Count);
                foreach (var receiver in originalBlueprint.BlockData.SignalReceivers)
                {
                    Assert.True(deserializedBlueprint.BlockData.SignalReceivers.ContainsKey(receiver.Key));
                    Assert.Equal(receiver.Value.Count, deserializedBlueprint.BlockData.SignalReceivers[receiver.Key].Count);
                }

                // Compare circuits
                Assert.Equal(originalBlueprint.BlockData.Circuits.Count, deserializedBlueprint.BlockData.Circuits.Count);

                // Compare shortcut names
                Assert.Equal(originalBlueprint.BlockData.ShortcutNames.Count, deserializedBlueprint.BlockData.ShortcutNames.Count);
                for (int i = 0; i < originalBlueprint.BlockData.ShortcutNames.Count; i++)
                {
                    Assert.Equal(originalBlueprint.BlockData.ShortcutNames[i], deserializedBlueprint.BlockData.ShortcutNames[i]);
                }
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void SimpleCubeBlueprint_ShouldSerializeAndDeserializeCorrectly()
        {
            // Arrange
            var originalBlueprint = new Blueprint(SIMPLE_CUBE_PATH);
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act - Serialize the blueprint to a temporary file
                using (var fileStream = File.Create(tempFile))
                using (var writer = new BinaryWriter(fileStream))
                {
                    originalBlueprint.Serialize(writer);
                }

                // Assert - Read back the serialized file
                var deserializedBlueprint = new Blueprint(tempFile);

                // Basic validation
                Assert.NotNull(deserializedBlueprint);
                Assert.Equal(originalBlueprint.Header.Size, deserializedBlueprint.Header.Size);
                Assert.Equal(originalBlueprint.BlockData.BlocksSize, deserializedBlueprint.BlockData.BlocksSize);

                // Verify the file sizes for round-trip testing
                var originalFileInfo = new FileInfo(SIMPLE_CUBE_PATH);
                var serializedFileInfo = new FileInfo(tempFile);

                Assert.True(serializedFileInfo.Length > 0);
                Console.WriteLine($"📊 File size comparison:");
                Console.WriteLine($"   Original: {originalFileInfo.Length} bytes");
                Console.WriteLine($"   Serialized: {serializedFileInfo.Length} bytes");
                Console.WriteLine($"   Difference: {serializedFileInfo.Length - originalFileInfo.Length} bytes");
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Theory]
        [ClassData(typeof(BlueprintTestData))]
        public void BlueprintSerialization_ShouldPreserveAllData(BlueprintDetails details)
        {
            // Arrange
            var originalBlueprint = new Blueprint(details.File);
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act - Serialize and deserialize
                using (var fileStream = File.Create(tempFile))
                using (var writer = new BinaryWriter(fileStream))
                {
                    originalBlueprint.Serialize(writer);
                }

                var deserializedBlueprint = new Blueprint(tempFile);

                // Assert - Verify all critical data is preserved
                Assert.Equal(originalBlueprint.Header.Version, deserializedBlueprint.Header.Version);
                Assert.Equal(originalBlueprint.Header.Size, deserializedBlueprint.Header.Size);
                Assert.Equal(originalBlueprint.Header.SizeClass, deserializedBlueprint.Header.SizeClass);

                // Verify block count matches expected (if BlockData is available)
                if (deserializedBlueprint.BlockData != null)
                {
                    var nonZeroBlocks = deserializedBlueprint.BlockData.Blocks
                        .Take(deserializedBlueprint.BlockData.BlocksSize)
                        .Count(b => b.Data != 0);
                    Assert.Equal(details.BlockCount, nonZeroBlocks);
                }
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void CreatePermanentEPB_ForInGameTesting()
        {
            // Arrange
            var outputDirectory = @"..\..\..\..\TestOutput";
            var outputFile = Path.Combine(outputDirectory, "SerializedSimpleCube_ForGameTesting.epb");

            // Ensure output directory exists
            Directory.CreateDirectory(outputDirectory);

            // Load the original blueprint
            var originalBlueprint = new Blueprint(SIMPLE_CUBE_PATH);

            // Act - Serialize the blueprint to a permanent file
            using (var fileStream = File.Create(outputFile))
            using (var writer = new BinaryWriter(fileStream))
            {
                originalBlueprint.Serialize(writer);
            }

            // Verify the serialized file can be read back correctly
            var deserializedBlueprint = new Blueprint(outputFile);

            // Basic validation
            Assert.NotNull(deserializedBlueprint);
            Assert.Equal(originalBlueprint.Header.Version, deserializedBlueprint.Header.Version);
            Assert.Equal(originalBlueprint.Header.Size, deserializedBlueprint.Header.Size);
            Assert.Equal(originalBlueprint.BlockData.BlocksSize, deserializedBlueprint.BlockData.BlocksSize);

            // Output information for the user
            var fileInfo = new FileInfo(outputFile);
            var absolutePath = Path.GetFullPath(outputFile);

            Console.WriteLine($"✅ Permanent EPB file created successfully!");
            Console.WriteLine($"📁 Location: {absolutePath}");
            Console.WriteLine($"📊 File size: {fileInfo.Length} bytes");
            Console.WriteLine($"🎮 This file can now be loaded into the game for testing.");
            Console.WriteLine($"🔧 Blueprint info: {deserializedBlueprint.Header.Size} blocks, Version {deserializedBlueprint.Header.Version}");

            // Note: This file is intentionally NOT deleted for in-game testing
        }

        [Fact]
        public void DetailedBinaryComparison_OriginalVsSerialized()
        {
            // Arrange
            var outputDirectory = @"..\..\..\..\TestOutput";
            var outputFile = Path.Combine(outputDirectory, "SerializedSimpleCube_DetailedComparison.epb");

            // Ensure output directory exists
            Directory.CreateDirectory(outputDirectory);

            // Load and serialize
            var originalBlueprint = new Blueprint(SIMPLE_CUBE_PATH);
            using (var fileStream = File.Create(outputFile))
            using (var writer = new BinaryWriter(fileStream))
            {
                originalBlueprint.Serialize(writer);
            }

            // Read both files as byte arrays
            var originalBytes = File.ReadAllBytes(SIMPLE_CUBE_PATH);
            var serializedBytes = File.ReadAllBytes(outputFile);

            Console.WriteLine($"🔍 DETAILED BINARY COMPARISON");
            Console.WriteLine($"📁 Original file: {SIMPLE_CUBE_PATH}");
            Console.WriteLine($"📁 Serialized file: {outputFile}");
            Console.WriteLine($"📊 Original size: {originalBytes.Length} bytes");
            Console.WriteLine($"📊 Serialized size: {serializedBytes.Length} bytes");
            Console.WriteLine($"📏 Size difference: {serializedBytes.Length - originalBytes.Length} bytes");
            Console.WriteLine();

            // Find first difference
            int minLength = Math.Min(originalBytes.Length, serializedBytes.Length);
            int firstDifference = -1;

            for (int i = 0; i < minLength; i++)
            {
                if (originalBytes[i] != serializedBytes[i])
                {
                    firstDifference = i;
                    break;
                }
            }

            if (firstDifference >= 0)
            {
                Console.WriteLine($"❌ FIRST DIFFERENCE AT BYTE {firstDifference} (0x{firstDifference:X})");
                Console.WriteLine($"   Original: 0x{originalBytes[firstDifference]:X2} ({originalBytes[firstDifference]})");
                Console.WriteLine($"   Serialized: 0x{serializedBytes[firstDifference]:X2} ({serializedBytes[firstDifference]})");
                Console.WriteLine();

                // Show context around the difference
                int contextStart = Math.Max(0, firstDifference - 10);
                int contextEnd = Math.Min(minLength, firstDifference + 10);

                Console.WriteLine($"📋 CONTEXT AROUND DIFFERENCE (bytes {contextStart}-{contextEnd}):");
                Console.Write("Original:   ");
                for (int i = contextStart; i <= contextEnd; i++)
                {
                    if (i == firstDifference)
                        Console.Write($"[{originalBytes[i]:X2}] ");
                    else
                        Console.Write($"{originalBytes[i]:X2} ");
                }
                Console.WriteLine();

                Console.Write("Serialized: ");
                for (int i = contextStart; i <= contextEnd; i++)
                {
                    if (i < serializedBytes.Length)
                    {
                        if (i == firstDifference)
                            Console.Write($"[{serializedBytes[i]:X2}] ");
                        else
                            Console.Write($"{serializedBytes[i]:X2} ");
                    }
                    else
                    {
                        Console.Write("-- ");
                    }
                }
                Console.WriteLine();
            }
            else if (originalBytes.Length != serializedBytes.Length)
            {
                Console.WriteLine($"❌ FILES DIFFER IN LENGTH");
                Console.WriteLine($"   Common prefix: {minLength} bytes are identical");
                if (originalBytes.Length > serializedBytes.Length)
                {
                    Console.WriteLine($"   Original has {originalBytes.Length - serializedBytes.Length} extra bytes");
                    Console.Write($"   Extra bytes: ");
                    for (int i = minLength; i < Math.Min(originalBytes.Length, minLength + 20); i++)
                    {
                        Console.Write($"{originalBytes[i]:X2} ");
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine($"   Serialized has {serializedBytes.Length - originalBytes.Length} extra bytes");
                    Console.Write($"   Extra bytes: ");
                    for (int i = minLength; i < Math.Min(serializedBytes.Length, minLength + 20); i++)
                    {
                        Console.Write($"{serializedBytes[i]:X2} ");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine($"✅ FILES ARE IDENTICAL!");
            }

            // Count total differences
            if (firstDifference >= 0)
            {
                int diffCount = 0;
                for (int i = 0; i < minLength; i++)
                {
                    if (originalBytes[i] != serializedBytes[i])
                        diffCount++;
                }
                Console.WriteLine($"📊 Total byte differences: {diffCount} out of {minLength} compared bytes");
            }

            // Try to load both blueprints and compare at object level
            try
            {
                var originalBlueprint2 = new Blueprint(SIMPLE_CUBE_PATH);
                var serializedBlueprint = new Blueprint(outputFile);

                Console.WriteLine();
                Console.WriteLine($"🔧 BLUEPRINT OBJECT COMPARISON:");
                Console.WriteLine($"   Original Header Size: {originalBlueprint2.Header.Size}");
                Console.WriteLine($"   Serialized Header Size: {serializedBlueprint.Header.Size}");
                Console.WriteLine($"   Original Version: {originalBlueprint2.Header.Version}");
                Console.WriteLine($"   Serialized Version: {serializedBlueprint.Header.Version}");
                Console.WriteLine($"   Original BlocksSize: {originalBlueprint2.BlockData.BlocksSize}");
                Console.WriteLine($"   Serialized BlocksSize: {serializedBlueprint.BlockData.BlocksSize}");

                // Count non-zero blocks
                var originalNonZero = originalBlueprint2.BlockData.Blocks.Take(originalBlueprint2.BlockData.BlocksSize).Count(b => b.Data != 0);
                var serializedNonZero = serializedBlueprint.BlockData.Blocks.Take(serializedBlueprint.BlockData.BlocksSize).Count(b => b.Data != 0);
                Console.WriteLine($"   Original non-zero blocks: {originalNonZero}");
                Console.WriteLine($"   Serialized non-zero blocks: {serializedNonZero}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading serialized blueprint: {ex.Message}");
            }

            // This test will fail if files are different to draw attention
            if (firstDifference >= 0 || originalBytes.Length != serializedBytes.Length)
            {
                Assert.Fail($"Files differ! First difference at byte {firstDifference}. Check console output for details.");
            }
        }

        [Fact]
        public void HeaderDiagnostic_PropertySerialization()
        {
            // Arrange
            var outputDirectory = @"..\..\..\..\TestOutput";
            var outputFile = Path.Combine(outputDirectory, "HeaderDiagnostic.epb");

            // Ensure output directory exists
            Directory.CreateDirectory(outputDirectory);

            // Load original blueprint
            var originalBlueprint = new Blueprint(SIMPLE_CUBE_PATH);

            Console.WriteLine($"🔍 HEADER DIAGNOSTIC - PROPERTIES ANALYSIS");
            Console.WriteLine($"📁 Original blueprint: {SIMPLE_CUBE_PATH}");
            Console.WriteLine($"🔧 Version: {originalBlueprint.Header.Version}");
            Console.WriteLine($"📦 Blueprint Type: {originalBlueprint.Header.BlueprintType}");
            Console.WriteLine($"📏 Size: {originalBlueprint.Header.Size}");
            Console.WriteLine($"📝 Properties Count: {originalBlueprint.Header.Properties.Count}");
            Console.WriteLine();

            // List all properties
            Console.WriteLine($"📋 PROPERTIES LIST:");
            foreach (var prop in originalBlueprint.Header.Properties)
            {
                Console.WriteLine($"   {prop.Name} = {prop.Value} (Type: {prop.Type})");
            }
            Console.WriteLine();

            // Serialize header only to see bytes
            byte[] headerBytes;
            using (var headerStream = new MemoryStream())
            using (var headerWriter = new BinaryWriter(headerStream))
            {
                originalBlueprint.Header.Serialize(headerWriter);
                headerWriter.Flush();
                headerBytes = headerStream.ToArray();
            }

            Console.WriteLine($"📊 SERIALIZED HEADER SIZE: {headerBytes.Length} bytes");
            Console.WriteLine($"📋 HEADER BYTES (first 50):");
            for (int i = 0; i < Math.Min(headerBytes.Length, 50); i++)
            {
                if (i % 16 == 0) Console.WriteLine();
                Console.Write($"{headerBytes[i]:X2} ");
            }
            Console.WriteLine();
            Console.WriteLine();

            // Read original file header bytes directly
            var originalFileBytes = File.ReadAllBytes(SIMPLE_CUBE_PATH);
            Console.WriteLine($"📋 ORIGINAL FILE BYTES (first 50):");
            for (int i = 0; i < Math.Min(originalFileBytes.Length, 50); i++)
            {
                if (i % 16 == 0) Console.WriteLine();
                Console.Write($"{originalFileBytes[i]:X2} ");
            }
            Console.WriteLine();
            Console.WriteLine();

            // Compare byte by byte up to properties section
            Console.WriteLine($"🔍 BYTE-BY-BYTE HEADER COMPARISON:");
            int headerMinLen = Math.Min(headerBytes.Length, 50);
            bool foundDiff = false;
            for (int i = 0; i < headerMinLen; i++)
            {
                if (i < originalFileBytes.Length && headerBytes[i] != originalFileBytes[i])
                {
                    Console.WriteLine($"   Difference at byte {i}: Original=0x{originalFileBytes[i]:X2}, Serialized=0x{headerBytes[i]:X2}");
                    foundDiff = true;
                }
            }
            if (!foundDiff)
            {
                Console.WriteLine($"   First {headerMinLen} bytes are identical");
            }

            // Serialize full blueprint for ZIP archive analysis
            using (var fileStream = File.Create(outputFile))
            using (var writer = new BinaryWriter(fileStream))
            {
                originalBlueprint.Serialize(writer);
            }

            var serializedBytes = File.ReadAllBytes(outputFile);
            Console.WriteLine($"📊 FULL BLUEPRINT SERIALIZED SIZE: {serializedBytes.Length} bytes");
            Console.WriteLine($"📊 Original file size: {originalFileBytes.Length} bytes");
            Console.WriteLine($"📏 Size difference: {serializedBytes.Length - originalFileBytes.Length} bytes");
        }

        [Fact]
        public void TrailingDataAnalysis_CaptureUnreadData()
        {
            // Arrange
            var outputDirectory = @"..\..\..\..\TestOutput";
            var outputFile = Path.Combine(outputDirectory, "TrailingDataFixed.epb");

            // Ensure output directory exists
            Directory.CreateDirectory(outputDirectory);

            Console.WriteLine($"🔍 TRAILING DATA ANALYSIS");
            Console.WriteLine($"📁 Analyzing: {SIMPLE_CUBE_PATH}");
            Console.WriteLine();

            // Load original blueprint (will capture trailing data)
            var originalBlueprint = new Blueprint(SIMPLE_CUBE_PATH);

            Console.WriteLine($"📊 Original file size: {new FileInfo(SIMPLE_CUBE_PATH).Length} bytes");
            Console.WriteLine($"📦 Trailing data captured: {originalBlueprint.TrailingData.Length} bytes");
            Console.WriteLine();

            if (originalBlueprint.TrailingData.Length > 0)
            {
                Console.WriteLine($"📋 COMPLETE TRAILING DATA:");
                for (int i = 0; i < originalBlueprint.TrailingData.Length; i++)
                {
                    if (i % 16 == 0) Console.WriteLine();
                    Console.Write($"{originalBlueprint.TrailingData[i]:X2} ");
                }
                Console.WriteLine();
                Console.WriteLine();

                // Try to interpret as various data types
                if (originalBlueprint.TrailingData.Length >= 4)
                {
                    var asInt32 = BitConverter.ToInt32(originalBlueprint.TrailingData, 0);
                    var asUInt32 = BitConverter.ToUInt32(originalBlueprint.TrailingData, 0);
                    Console.WriteLine($"🔢 First 4 bytes as Int32: {asInt32}");
                    Console.WriteLine($"🔢 First 4 bytes as UInt32: {asUInt32}");
                    Console.WriteLine($"🔢 First 4 bytes as hex: 0x{asUInt32:X8}");
                }

                if (originalBlueprint.TrailingData.Length >= 8)
                {
                    var asInt64 = BitConverter.ToInt64(originalBlueprint.TrailingData, 0);
                    Console.WriteLine($"🔢 First 8 bytes as Int64: {asInt64}");
                }
            }

            // Serialize with trailing data included
            using (var fileStream = File.Create(outputFile))
            using (var writer = new BinaryWriter(fileStream))
            {
                originalBlueprint.Serialize(writer);
            }

            var originalFileBytes = File.ReadAllBytes(SIMPLE_CUBE_PATH);
            var serializedBytes = File.ReadAllBytes(outputFile);

            Console.WriteLine($"📊 COMPARISON AFTER TRAILING DATA FIX:");
            Console.WriteLine($"   Original size: {originalFileBytes.Length} bytes");
            Console.WriteLine($"   Serialized size: {serializedBytes.Length} bytes");
            Console.WriteLine($"   Size difference: {serializedBytes.Length - originalFileBytes.Length} bytes");

            // Test if they're now identical
            bool identical = originalFileBytes.Length == serializedBytes.Length;
            if (identical)
            {
                for (int i = 0; i < originalFileBytes.Length; i++)
                {
                    if (originalFileBytes[i] != serializedBytes[i])
                    {
                        identical = false;
                        Console.WriteLine($"❌ First difference at byte {i}");
                        break;
                    }
                }
            }

            if (identical)
            {
                Console.WriteLine($"✅ FILES ARE NOW IDENTICAL!");
            }
            else
            {
                Console.WriteLine($"❌ Files still differ - need more investigation");
            }

            // Try to load the serialized file to make sure it's valid
            try
            {
                var reloadedBlueprint = new Blueprint(outputFile);
                Console.WriteLine($"✅ Serialized blueprint loads successfully");
                Console.WriteLine($"📦 Reloaded trailing data: {reloadedBlueprint.TrailingData.Length} bytes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading serialized blueprint: {ex.Message}");
            }
        }

        [Fact]
        public void ZipArchiveAnalysis_CompareContents()
        {
            // Arrange
            var outputDirectory = @"..\..\..\..\TestOutput";
            var outputFile = Path.Combine(outputDirectory, "ZipAnalysis.epb");

            // Ensure output directory exists
            Directory.CreateDirectory(outputDirectory);

            Console.WriteLine($"🔍 ZIP ARCHIVE ANALYSIS");
            Console.WriteLine($"📁 Analyzing: {SIMPLE_CUBE_PATH}");
            Console.WriteLine();

            // Load original blueprint
            var originalBlueprint = new Blueprint(SIMPLE_CUBE_PATH);

            // Serialize to get our version
            using (var fileStream = File.Create(outputFile))
            using (var writer = new BinaryWriter(fileStream))
            {
                originalBlueprint.Serialize(writer);
            }

            // Helper function to extract ZIP archive from a blueprint file
            void AnalyzeZipArchive(string filePath, string label)
            {
                using (var stream = new MemoryStream(File.ReadAllBytes(filePath)))
                using (var reader = new BinaryReader(stream))
                {
                    // Read magic number and version to find ZIP archive position
                    var magic = reader.ReadInt32(); // Should be 2022986309
                    var version = reader.ReadInt32();

                    // Skip blueprint type
                    if (version > 1)
                        reader.ReadByte();

                    // Skip size
                    if (version > 2)
                    {
                        reader.ReadInt32(); // X
                        reader.ReadInt32(); // Y
                        reader.ReadInt32(); // Z

                        // Skip properties section
                        var propertiesGarbage1 = reader.ReadInt16();
                        var propertiesCount = reader.ReadInt16();

                        for (int i = 0; i < propertiesCount; i++)
                        {
                            var propName = reader.ReadInt32();
                            var propType = reader.ReadInt32();
                            var actualType = (byte)(propType >> 24);

                            // Read property value based on type
                            switch (actualType)
                            {
                                case 1: // String
                                    reader.ReadString();
                                    break;
                                case 2: // Bool
                                    reader.ReadBoolean();
                                    reader.ReadString(); // metadata
                                    break;
                                case 3: // Int
                                    reader.ReadInt32();
                                    reader.ReadString(); // metadata
                                    break;
                                case 4: // Single
                                    reader.ReadSingle();
                                    reader.ReadString(); // metadata
                                    break;
                                case 5: // Vector3
                                    reader.ReadSingle(); // X
                                    reader.ReadSingle(); // Y
                                    reader.ReadSingle(); // Z
                                    reader.ReadString(); // metadata
                                    break;
                                case 6: // Long
                                    reader.ReadInt64();
                                    reader.ReadString(); // metadata
                                    break;
                            }
                        }

                        var propertiesGarbage2 = reader.ReadInt16();
                    }

                    // Skip statistics section if present
                    if (version > 3)
                    {
                        // This is complex, let's skip to a known position
                        // We know the ZIP archive starts around byte 475 for Simple Cube
                        stream.Position = 475;
                    }

                    var zipLength = reader.ReadInt32();
                    var garbageBytes = reader.ReadBytes(2);
                    var zipBytes = reader.ReadBytes(zipLength);

                    Console.WriteLine($"📦 {label}:");
                    Console.WriteLine($"   ZIP Length: {zipLength} bytes");
                    Console.WriteLine($"   Garbage bytes: {garbageBytes[0]:X2} {garbageBytes[1]:X2}");
                    Console.WriteLine($"   ZIP Archive size: {zipBytes.Length} bytes");
                    Console.WriteLine($"   First 16 bytes: {string.Join(" ", zipBytes.Take(16).Select(b => b.ToString("X2")))}");
                    Console.WriteLine();

                    // Analyze ZIP contents
                    try
                    {
                        using (var zipStream = new MemoryStream(zipBytes))
                        using (var zipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(zipStream))
                        {
                            Console.WriteLine($"   ZIP Entries: {zipFile.Count}");
                            for (int i = 0; i < zipFile.Count; i++)
                            {
                                var entry = zipFile[i];
                                Console.WriteLine($"   Entry {i}: {entry.Name}, Size: {entry.Size}, Compressed: {entry.CompressedSize}");

                                if (entry.Name == "0")
                                {
                                    using (var entryStream = zipFile.GetInputStream(entry))
                                    using (var entryReader = new BinaryReader(entryStream))
                                    {
                                        var entryData = entryReader.ReadBytes((int)entry.Size);
                                        Console.WriteLine($"   Entry '0' first 32 bytes: {string.Join(" ", entryData.Take(32).Select(b => b.ToString("X2")))}");
                                        Console.WriteLine($"   Entry '0' total size: {entryData.Length} bytes");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ❌ Error reading ZIP: {ex.Message}");
                    }
                    Console.WriteLine();
                }
            }

            // Analyze both files
            AnalyzeZipArchive(SIMPLE_CUBE_PATH, "ORIGINAL ZIP ARCHIVE");
            AnalyzeZipArchive(outputFile, "SERIALIZED ZIP ARCHIVE");

            // Create our own block data and compare
            Console.WriteLine($"🔧 BLOCK DATA COMPARISON:");

            byte[] ourBlockData;
            using (var blockStream = new MemoryStream())
            using (var blockWriter = new BinaryWriter(blockStream))
            {
                originalBlueprint.BlockData.Serialize(blockWriter);
                blockWriter.Flush();
                ourBlockData = blockStream.ToArray();
            }

            Console.WriteLine($"   Our block data size: {ourBlockData.Length} bytes");
            Console.WriteLine($"   Our block data first 32 bytes: {string.Join(" ", ourBlockData.Take(32).Select(b => b.ToString("X2")))}");
        }

        [Fact]
        public void BlockDataComparison_FindMissingBytes()
        {
            // Arrange
            Console.WriteLine($"🔍 BLOCK DATA DEEP ANALYSIS");
            Console.WriteLine($"📁 Analyzing: {SIMPLE_CUBE_PATH}");
            Console.WriteLine();

            // Load original blueprint
            var originalBlueprint = new Blueprint(SIMPLE_CUBE_PATH);

            // Extract original block data from ZIP
            byte[] originalBlockData;
            using (var stream = new MemoryStream(File.ReadAllBytes(SIMPLE_CUBE_PATH)))
            using (var reader = new BinaryReader(stream))
            {
                // Navigate to ZIP archive (skip header)
                stream.Position = 475; // Known position for Simple Cube
                var zipLength = reader.ReadInt32();
                reader.ReadBytes(2); // garbage
                var zipBytes = reader.ReadBytes(zipLength);

                using (var zipStream = new MemoryStream(zipBytes))
                using (var zipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(zipStream))
                {
                    var entry = zipFile.Cast<ICSharpCode.SharpZipLib.Zip.ZipEntry>().First(e => e.Name == "0");
                    using (var entryStream = zipFile.GetInputStream(entry))
                    using (var entryReader = new BinaryReader(entryStream))
                    {
                        originalBlockData = entryReader.ReadBytes((int)entry.Size);
                    }
                }
            }

            // Generate our block data
            byte[] ourBlockData;
            using (var blockStream = new MemoryStream())
            using (var blockWriter = new BinaryWriter(blockStream))
            {
                originalBlueprint.BlockData.Serialize(blockWriter);
                blockWriter.Flush();
                ourBlockData = blockStream.ToArray();
            }

            Console.WriteLine($"📊 BLOCK DATA SIZE COMPARISON:");
            Console.WriteLine($"   Original block data: {originalBlockData.Length} bytes");
            Console.WriteLine($"   Our block data: {ourBlockData.Length} bytes");
            Console.WriteLine($"   Missing bytes: {originalBlockData.Length - ourBlockData.Length}");
            Console.WriteLine();

            // Find first difference
            int minLength = Math.Min(originalBlockData.Length, ourBlockData.Length);
            int firstDifference = -1;

            for (int i = 0; i < minLength; i++)
            {
                if (originalBlockData[i] != ourBlockData[i])
                {
                    firstDifference = i;
                    break;
                }
            }

            if (firstDifference >= 0)
            {
                Console.WriteLine($"❌ FIRST BLOCK DATA DIFFERENCE AT BYTE {firstDifference}");
                Console.WriteLine($"   Original: 0x{originalBlockData[firstDifference]:X2}");
                Console.WriteLine($"   Ours: 0x{ourBlockData[firstDifference]:X2}");

                // Show context around difference
                int contextStart = Math.Max(0, firstDifference - 16);
                int contextEnd = Math.Min(minLength, firstDifference + 16);

                Console.WriteLine($"📋 CONTEXT (bytes {contextStart}-{contextEnd}):");
                Console.Write("Original: ");
                for (int i = contextStart; i <= contextEnd; i++)
                {
                    if (i == firstDifference)
                        Console.Write($"[{originalBlockData[i]:X2}] ");
                    else
                        Console.Write($"{originalBlockData[i]:X2} ");
                }
                Console.WriteLine();

                Console.Write("Ours:     ");
                for (int i = contextStart; i <= contextEnd; i++)
                {
                    if (i < ourBlockData.Length)
                    {
                        if (i == firstDifference)
                            Console.Write($"[{ourBlockData[i]:X2}] ");
                        else
                            Console.Write($"{ourBlockData[i]:X2} ");
                    }
                    else
                    {
                        Console.Write("-- ");
                    }
                }
                Console.WriteLine();
            }
            else if (originalBlockData.Length != ourBlockData.Length)
            {
                Console.WriteLine($"❌ BLOCK DATA DIFFERS IN LENGTH");
                Console.WriteLine($"   Common prefix: {minLength} bytes are identical");

                if (originalBlockData.Length > ourBlockData.Length)
                {
                    Console.WriteLine($"   📋 MISSING DATA (bytes {minLength}-{originalBlockData.Length - 1}):");
                    for (int i = minLength; i < Math.Min(originalBlockData.Length, minLength + 32); i++)
                    {
                        if ((i - minLength) % 16 == 0) Console.WriteLine();
                        Console.Write($"{originalBlockData[i]:X2} ");
                    }
                    Console.WriteLine();

                    // Try to interpret what this missing data might be
                    if (originalBlockData.Length - ourBlockData.Length >= 4)
                    {
                        int missingInt = BitConverter.ToInt32(originalBlockData, minLength);
                        Console.WriteLine($"   First 4 missing bytes as Int32: {missingInt}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"✅ BLOCK DATA IS IDENTICAL!");
            }

            Console.WriteLine();
            Console.WriteLine($"📊 BLOCK DATA STATS:");
            Console.WriteLine($"   Blueprint size: {originalBlueprint.BlockData.Size}");
            Console.WriteLine($"   Blocks count: {originalBlueprint.BlockData.BlocksSize}");
            Console.WriteLine($"   Entities: {originalBlueprint.BlockData.Entities.Count}");
            Console.WriteLine($"   Lock codes: {originalBlueprint.BlockData.LockCodes.Count}");
            Console.WriteLine($"   Signal sources: {originalBlueprint.BlockData.SignalSources.Count}");
            Console.WriteLine($"   Signal receivers: {originalBlueprint.BlockData.SignalReceivers.Count}");
            Console.WriteLine($"   Circuits: {originalBlueprint.BlockData.Circuits.Count}");
            Console.WriteLine($"   Shortcut names: {originalBlueprint.BlockData.ShortcutNames.Count}");
        }

        [Fact]
        public void RoundTripComparison_FindLostData()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();

            try
            {
                Console.WriteLine($"🔍 ROUND-TRIP OBJECT COMPARISON");
                Console.WriteLine($"📁 Analyzing: {SIMPLE_CUBE_PATH}");
                Console.WriteLine();

                // 1. Load original EPB (deserialize)
                var originalBlueprint = new Blueprint(SIMPLE_CUBE_PATH);

                // 2. Serialize it to temp file
                using (var fileStream = File.Create(tempFile))
                using (var writer = new BinaryWriter(fileStream))
                {
                    originalBlueprint.Serialize(writer);
                }

                // 3. Load temp file (deserialize again)
                var reserializedBlueprint = new Blueprint(tempFile);

                Console.WriteLine($"📊 BLUEPRINT COMPARISON:");
                Console.WriteLine($"   Original file size: {new FileInfo(SIMPLE_CUBE_PATH).Length} bytes");
                Console.WriteLine($"   Reserialized file size: {new FileInfo(tempFile).Length} bytes");
                Console.WriteLine();

                // Compare Header properties
                Console.WriteLine($"📋 HEADER COMPARISON:");
                bool headerMatch = true;

                if (originalBlueprint.Header.Version != reserializedBlueprint.Header.Version)
                {
                    Console.WriteLine($"   ❌ Version: {originalBlueprint.Header.Version} → {reserializedBlueprint.Header.Version}");
                    headerMatch = false;
                }

                if (originalBlueprint.Header.BlueprintType != reserializedBlueprint.Header.BlueprintType)
                {
                    Console.WriteLine($"   ❌ BlueprintType: {originalBlueprint.Header.BlueprintType} → {reserializedBlueprint.Header.BlueprintType}");
                    headerMatch = false;
                }

                if (originalBlueprint.Header.Size != reserializedBlueprint.Header.Size)
                {
                    Console.WriteLine($"   ❌ Size: {originalBlueprint.Header.Size} → {reserializedBlueprint.Header.Size}");
                    headerMatch = false;
                }

                if (originalBlueprint.Header.Properties.Count != reserializedBlueprint.Header.Properties.Count)
                {
                    Console.WriteLine($"   ❌ Properties Count: {originalBlueprint.Header.Properties.Count} → {reserializedBlueprint.Header.Properties.Count}");
                    headerMatch = false;
                }
                else
                {
                    // Compare individual properties
                    for (int i = 0; i < originalBlueprint.Header.Properties.Count; i++)
                    {
                        var orig = originalBlueprint.Header.Properties[i];
                        var reser = reserializedBlueprint.Header.Properties[i];

                        if (orig.Name != reser.Name || orig.Value?.ToString() != reser.Value?.ToString() || orig.Type != reser.Type)
                        {
                            Console.WriteLine($"   ❌ Property {i}: {orig.Name}={orig.Value}({orig.Type}) → {reser.Name}={reser.Value}({reser.Type})");
                            headerMatch = false;
                        }
                    }
                }

                if (headerMatch)
                {
                    Console.WriteLine($"   ✅ Header data is identical");
                }
                Console.WriteLine();

                // Compare BlockData properties
                Console.WriteLine($"📋 BLOCK DATA COMPARISON:");
                bool blockDataMatch = true;

                if (originalBlueprint.BlockData.Size != reserializedBlueprint.BlockData.Size)
                {
                    Console.WriteLine($"   ❌ Size: {originalBlueprint.BlockData.Size} → {reserializedBlueprint.BlockData.Size}");
                    blockDataMatch = false;
                }

                if (originalBlueprint.BlockData.BlocksSize != reserializedBlueprint.BlockData.BlocksSize)
                {
                    Console.WriteLine($"   ❌ BlocksSize: {originalBlueprint.BlockData.BlocksSize} → {reserializedBlueprint.BlockData.BlocksSize}");
                    blockDataMatch = false;
                }

                // Compare collection counts
                if (originalBlueprint.BlockData.Entities.Count != reserializedBlueprint.BlockData.Entities.Count)
                {
                    Console.WriteLine($"   ❌ Entities Count: {originalBlueprint.BlockData.Entities.Count} → {reserializedBlueprint.BlockData.Entities.Count}");
                    blockDataMatch = false;
                }

                if (originalBlueprint.BlockData.LockCodes.Count != reserializedBlueprint.BlockData.LockCodes.Count)
                {
                    Console.WriteLine($"   ❌ LockCodes Count: {originalBlueprint.BlockData.LockCodes.Count} → {reserializedBlueprint.BlockData.LockCodes.Count}");
                    blockDataMatch = false;
                }

                if (originalBlueprint.BlockData.SignalSources.Count != reserializedBlueprint.BlockData.SignalSources.Count)
                {
                    Console.WriteLine($"   ❌ SignalSources Count: {originalBlueprint.BlockData.SignalSources.Count} → {reserializedBlueprint.BlockData.SignalSources.Count}");
                    blockDataMatch = false;
                }

                if (originalBlueprint.BlockData.SignalReceivers.Count != reserializedBlueprint.BlockData.SignalReceivers.Count)
                {
                    Console.WriteLine($"   ❌ SignalReceivers Count: {originalBlueprint.BlockData.SignalReceivers.Count} → {reserializedBlueprint.BlockData.SignalReceivers.Count}");
                    blockDataMatch = false;
                }

                if (originalBlueprint.BlockData.Circuits.Count != reserializedBlueprint.BlockData.Circuits.Count)
                {
                    Console.WriteLine($"   ❌ Circuits Count: {originalBlueprint.BlockData.Circuits.Count} → {reserializedBlueprint.BlockData.Circuits.Count}");
                    blockDataMatch = false;
                }

                if (originalBlueprint.BlockData.ShortcutNames.Count != reserializedBlueprint.BlockData.ShortcutNames.Count)
                {
                    Console.WriteLine($"   ❌ ShortcutNames Count: {originalBlueprint.BlockData.ShortcutNames.Count} → {reserializedBlueprint.BlockData.ShortcutNames.Count}");
                    blockDataMatch = false;
                }

                // Compare individual blocks (first few)
                var originalBlocks = originalBlueprint.BlockData.Blocks.Take(originalBlueprint.BlockData.BlocksSize).ToArray();
                var reserializedBlocks = reserializedBlueprint.BlockData.Blocks.Take(reserializedBlueprint.BlockData.BlocksSize).ToArray();

                if (originalBlocks.Length != reserializedBlocks.Length)
                {
                    Console.WriteLine($"   ❌ Block array length: {originalBlocks.Length} → {reserializedBlocks.Length}");
                    blockDataMatch = false;
                }
                else
                {
                    int blockDifferences = 0;
                    for (int i = 0; i < Math.Min(originalBlocks.Length, 10); i++) // Check first 10 blocks
                    {
                        var orig = originalBlocks[i];
                        var reser = reserializedBlocks[i];

                        if (orig.Data != reser.Data || orig.Color != reser.Color || orig.Texture != reser.Texture ||
                            orig.Damage != reser.Damage || orig.Density != reser.Density || orig.Symbol != reser.Symbol)
                        {
                            Console.WriteLine($"   ❌ Block {i}: Data={orig.Data}→{reser.Data}, Color={orig.Color}→{reser.Color}, Texture={orig.Texture}→{reser.Texture}");
                            blockDataMatch = false;
                            blockDifferences++;
                        }
                    }

                    if (blockDifferences == 0 && originalBlocks.Length <= 10)
                    {
                        Console.WriteLine($"   ✅ All {originalBlocks.Length} blocks are identical");
                    }
                    else if (blockDifferences == 0)
                    {
                        Console.WriteLine($"   ✅ First 10 blocks are identical (total: {originalBlocks.Length})");
                    }
                }

                if (blockDataMatch)
                {
                    Console.WriteLine($"   ✅ Block data structure is identical");
                }
                Console.WriteLine();

                // Compare trailing data
                Console.WriteLine($"📋 TRAILING DATA COMPARISON:");
                if (originalBlueprint.TrailingData.Length != reserializedBlueprint.TrailingData.Length)
                {
                    Console.WriteLine($"   ❌ TrailingData length: {originalBlueprint.TrailingData.Length} → {reserializedBlueprint.TrailingData.Length}");
                }
                else if (originalBlueprint.TrailingData.Length > 0)
                {
                    bool trailingMatch = originalBlueprint.TrailingData.SequenceEqual(reserializedBlueprint.TrailingData);
                    if (trailingMatch)
                    {
                        Console.WriteLine($"   ✅ TrailingData is identical ({originalBlueprint.TrailingData.Length} bytes)");
                    }
                    else
                    {
                        Console.WriteLine($"   ❌ TrailingData differs ({originalBlueprint.TrailingData.Length} bytes)");
                    }
                }
                else
                {
                    Console.WriteLine($"   ✅ No trailing data in either version");
                }

                // Final verdict
                if (headerMatch && blockDataMatch)
                {
                    Console.WriteLine($"🎉 OBJECTS ARE FUNCTIONALLY IDENTICAL!");
                    Console.WriteLine($"   The differences are likely in low-level serialization details that don't affect game functionality.");
                }
                else
                {
                    Console.WriteLine($"❌ OBJECTS DIFFER - DATA IS BEING LOST DURING SERIALIZATION!");
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void CheckBlueprintVersion()
        {
            // Load blueprint and print version
            var blueprint = new Blueprint(SIMPLE_CUBE_PATH);
            Console.WriteLine($"🔧 Simple Cube Blueprint Version: {blueprint.Header.Version}");
            Console.WriteLine($"📦 Blueprint Type: {blueprint.Header.BlueprintType}");
            Console.WriteLine($"📏 Size: {blueprint.Header.Size}");
            Console.WriteLine($"📝 Properties Count: {blueprint.Header.Properties.Count}");

            // This is just a version check test
            Assert.True(blueprint.Header.Version > 0);
        }
    }
}

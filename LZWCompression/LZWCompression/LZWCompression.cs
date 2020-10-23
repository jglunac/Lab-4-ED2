using System;
using System.Collections.Generic;
using System.Text;

namespace LZWCompression
{
    public class LZWCompression
    {
        public string OriginalName { get; set; }
        public string CompressedFilePath { get; set; }
        public double CompressionRatio { get; set; }
        public double CompressionFactor { get; set; }
        public double ReductionPercentage { get; set; }
    }
}

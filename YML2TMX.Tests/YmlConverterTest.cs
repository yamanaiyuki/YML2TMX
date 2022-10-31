using System.Diagnostics;
using Yml2Tmx;

namespace YML2TMX.Tests
{
    public class YmlConverterTest
    {
        private readonly YmlConverter converter;

        public YmlConverterTest()
        {
            converter = new YmlConverter();
            Trace.WriteLine("YmlConverterTest Initialized");
        }

        [Fact]
        public void Test1()
        {
            string infile1 = @"original\evergreen_l_english.yml";
            string infile2 = @"translation\evergreen_l_english.yml";
            string outfile = @"evergreen_l_english.tmx";

            converter.ConvertTML(infile1, infile2, outfile);
        }

        [Fact]
        public void Test2()
        {
            string infile1 = @"original\evergreen_l_english.yml";
            string infile2 = @"translation\evergreen_l_english.yml";
            string outfile = @"evergreen_l_english.txt";

            converter.ConvertGlossary(infile1, infile2, outfile);
        }

        [Fact]
        public void Test3()
        {
            string infile = @"toml\test3.toml";

            converter.ConvertToml(infile);
        }

        [Fact]
        public void Test4()
        {
            string infile = @"toml\test4.toml";

            converter.ConvertToml(infile);
        }

        [Fact]
        public void Test5()
        {
            string infile = @"toml\test5.toml";

            converter.ConvertToml(infile);
        }

        [Fact]
        public void Test6()
        {
            string infile = @"toml\test6.toml";

            converter.ConvertToml(infile);
        }

        [Fact]
        public void Test7()
        {
            string infile = @"toml\test7.toml";

            converter.ConvertToml(infile);
        }

        [Fact]
        public void Test8()
        {
            string infile = @"toml\test8.toml";

            converter.ConvertToml(infile);
        }
    }
}
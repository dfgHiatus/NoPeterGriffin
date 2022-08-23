namespace NoPeterGriffin.Tests
{
    public class Program
    {
        private static AudioFingerprinter fingerprinter;
        static void Main(string[] args)
        {
            fingerprinter = new AudioFingerprinter();
            string testFile = "my_file.wav";
            Foo(testFile);
        }

        public async static void Foo(string file)
        {
            var res = await fingerprinter.ContainsMatches(file) ? "Found a match!" : "No match";
            Console.WriteLine(res);
        }
    }
}
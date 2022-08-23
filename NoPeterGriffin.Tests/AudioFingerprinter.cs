using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.InMemory;
using System.Security.Cryptography;

namespace NoPeterGriffin.Tests
{
    public class AudioFingerprinter
    {
        private readonly IModelService modelService = new InMemoryModelService(); // store fingerprints in RAM
        private readonly IAudioService audioService = new SoundFingerprintingAudioService(); // default audio library
        private string peterPath = Path.Combine("nml_mods", "peterSet_v1");

        public AudioFingerprinter()
        {
            // Load Peter moaning
            foreach (var file in Directory.GetFiles(Path.GetFullPath(peterPath), "*", SearchOption.AllDirectories))
            {
                PrepareAudio(file);
            }
        }

        public async void PrepareAudio(string file)
        {
            await StoreForLaterRetrieval(file).ConfigureAwait(false);
        }

        public async Task StoreForLaterRetrieval(string file)
        {
            var track = new TrackInfo(GenerateMD5(file), "", "");

            // create fingerprints
            var avHashes = await FingerprintCommandBuilder.Instance
                                        .BuildFingerprintCommand()
                                        .From(file)
                                        .UsingServices(audioService)
                                        .Hash();

            // store hashes in the database for later retrieval
            modelService.Insert(track, avHashes);
        }

        public async Task<bool> ContainsMatches(string file)
        {
            int secondsToAnalyze = 10; // number of seconds to analyze from query file
            int startAtSecond = 0; // start at the begining

            // query the underlying database for similar audio sub-fingerprints
            var queryResult = await QueryCommandBuilder.Instance.BuildQueryCommand()
                                                 .From(file, secondsToAnalyze, startAtSecond)
                                                 .UsingServices(modelService, audioService)
                                                 .Query();
            return queryResult.ContainsMatches;
        }

        //credit to delta for this method https://github.com/XDelta/
        private string GenerateMD5(string filepath)
        {
            using var hasher = MD5.Create();
            using var stream = File.OpenRead(filepath);
            var hash = hasher.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MoonXAltImporter {
    internal class Program {
        public static void Main() {
            var moonFolder = Environment.GetEnvironmentVariable("APPDATA") + "\\.minecraft\\Moon";
            var altsJsonPath = Path.Combine(moonFolder, "alts.json");
            if (!File.Exists(altsJsonPath)) {
                Console.WriteLine($"Couldn't find {altsJsonPath}");
                return;
            }

            var copyFilePath = Path.Combine(moonFolder, $"alts.{DateTime.Now.ToFileTime()}.json");
            File.Copy(altsJsonPath, copyFilePath);

            inputAltsFile:
            Console.Write("Please input the file to import> ");
            var importFilePath = Console.ReadLine();
            if (importFilePath == null || !File.Exists(importFilePath)) {
                Console.WriteLine($"File \"{importFilePath}\" doesn't exist.");
                goto inputAltsFile;
            }

            TextReader altsFileReader = new StreamReader(altsJsonPath);
            TextReader importFileReader = new StreamReader(importFilePath);

            var altsFile = JsonConvert.DeserializeObject<AltsFile>(altsFileReader.ReadToEnd());
            altsFileReader.Dispose();
            
            if (altsFile == null) {
                Console.WriteLine("Couldn't parse MoonX alts file!");
                return;
            }

            var altsAdded = 0;
            string line;
            while ((line = importFileReader.ReadLine()) != null) {
                var split = line.Split(':');
                if (split.Length != 2) continue;
                if (altsFile.Accounts.Exists(acc => acc.Email.Equals(split[0]))) continue;

                var account = new Account {Email = split[0], Name = split[0], Password = split[1], BannedTime = 0};
                altsFile.Accounts.Add(account);
                altsAdded++;
            }
            
            importFileReader.Dispose();

            var outputJson = JsonConvert.SerializeObject(altsFile);
            File.Delete(altsJsonPath);
            File.WriteAllText(altsJsonPath, outputJson);

            Console.WriteLine($"Added {altsAdded} alts");
        }
    }

    internal class Account {
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("password")] public string Password { get; set; }
        [JsonProperty("banned-time")] public int BannedTime { get; set; }
    }

    internal class AltsFile {
        [JsonProperty("lastalt")] public Account LastAlt { get; set; }
        [JsonProperty("accounts")] public List<Account> Accounts { get; set; }
    }
}
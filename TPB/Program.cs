using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace TorrentDownloader
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            HttpClient client = new HttpClient();

            // Search query
            Console.WriteLine("Enter search term:");
            string query = Console.ReadLine();

            // Pirate Bay API endpoint
            string url = $"https://apibay.org/q.php?q={Uri.EscapeDataString(query)}&cat=";

            // Send HTTP request
            HttpResponseMessage response = await client.GetAsync(url);

            // Parse JSON response
            string json = await response.Content.ReadAsStringAsync();
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            // Display search results
            int count = 0;
            foreach (var item in data)
            {
                count++;
                string name = item.name;
                int seeders = item.seeders;
                int leechers = item.leechers;
                long size = item.size;
                string username = item.username;
                string infoHash = item.info_hash;

                // Generate magnet link
                string magnetLink = GenerateMagnetLink(infoHash, name);

                // Add trackers to magnet link
                magnetLink = AddTrackersToMagnetLink(magnetLink);

                // Display information
                Console.WriteLine($"[{count}] Name: {name}");
                Console.WriteLine($"    Seeders: {seeders}");
                Console.WriteLine($"    Leechers: {leechers}");
                Console.WriteLine($"    Size: {size}");
                Console.WriteLine($"    Username: {username}");
                Console.WriteLine($"    Magnet link: {magnetLink}");
                Console.WriteLine();
            }

            // Ask user to select a torrent
            Console.WriteLine($"Found {count} torrents. Enter the number of the torrent you want to download:");
            int selection = int.Parse(Console.ReadLine());

            // Get the magnet link of the selected torrent
            var selectedTorrent = data[selection - 1];
            string selectedName = selectedTorrent.name;
            string selectedInfoHash = selectedTorrent.info_hash;
            string selectedMagnetLink = GenerateMagnetLink(selectedInfoHash, selectedName);
            selectedMagnetLink = AddTrackersToMagnetLink(selectedMagnetLink);

            // Open the magnet link in the default web browser
            Console.WriteLine($"Downloading {selectedName}...");
            Process.Start(new ProcessStartInfo(selectedMagnetLink) { UseShellExecute = true });
        }

        static string GenerateMagnetLink(string infoHashHex, string displayName)
        {
            // Convert infohash to byte array
            byte[] infoHashBytes = new byte[20];
            for (int i = 0; i < 20; i++)
            {
                infoHashBytes[i] = Convert.ToByte(infoHashHex.Substring(i * 2, 2), 16);
            }

            // Convert display name to UTF-8 bytes
            byte[] displayNameBytes = Encoding.UTF8.GetBytes(displayName);

            // Calculate SHA1 hash of the infohash and display name bytes
            SHA1 sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(infoHashBytes.Concat(displayNameBytes).ToArray());

            // Convert infohash and display name to hexadecimal strings
            string infoHash = BitConverter.ToString(infoHashBytes).Replace("-", "").ToLower();
            string displayNameEncoded = Uri.EscapeDataString(displayName);

            // Construct magnet link
            string magnetLink = $"magnet:?xt=urn:btih:{infoHash}&dn={displayNameEncoded}";

            return magnetLink;
        }

        static string AddTrackersToMagnetLink(string magnetLink)
        {
            // Tracker URLs
            string[] trackers = new string[] {
            "udp://tracker.openbittorrent.com:80",
            "udp://tracker.opentrackr.org:1337",
            "udp://tracker.coppersurfer.tk:6969"
        };

            // Concatenate tracker URLs
            string trackersString = "";
            for (int i = 0; i < trackers.Length; i++)
            {
                trackersString += "&tr=" + Uri.EscapeDataString(trackers[i]);
            }

            // Add trackers to magnet link
            magnetLink += trackersString;

            return magnetLink;
        }
    }
}

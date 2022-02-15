using iText.Kernel.Pdf;
using System;
using System.IO;
using System.Text;

namespace PDFDecrypter
{
    class Program
    {
        private static void DecryptFile(string inputFile, string outputFile, string password)
        {
            PdfReader reader;
            if (string.IsNullOrEmpty(password))
                reader = new PdfReader(inputFile);
            else
            {
                var props = new ReaderProperties();
                props.SetPassword(Encoding.Default.GetBytes(password));
                reader = new PdfReader(inputFile, props);
            }
            reader.SetUnethicalReading(true);

            var wprops = new WriterProperties();
            var writer = new PdfWriter(outputFile, wprops);
            var doc = new PdfDocument(reader, writer);
            doc.SetCloseReader(true);
            doc.SetCloseWriter(true);
            doc.Close();

            var ts = File.GetCreationTimeUtc(inputFile);
            File.SetCreationTimeUtc(outputFile, ts);
            ts = File.GetLastWriteTimeUtc(inputFile);
            File.SetLastWriteTimeUtc(outputFile, ts);
        }

        private static bool AskPath(string pathTypeName, out string path, bool mustexist, string defpath = "")
        {
            while (true)
            {
                Console.Write($"Specify {pathTypeName} path{(string.IsNullOrEmpty(defpath) ? "" : $" (default: {defpath})")}: ");
                path = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(path))
                    return false;
                if (!mustexist || Directory.Exists(path))
                    return true;
                Console.WriteLine($"- invalid {pathTypeName} path");
            }
        }

        static void Main(string[] args)
        {
            string srcPath;
            string[] srcPdfs;
            while (true)
            {
                if (!AskPath("source", out srcPath, true))
                    return;
                srcPdfs = Directory.GetFiles(srcPath, "*.pdf");
                if (srcPdfs.Length > 0)
                    break;
                Console.WriteLine($"- no PDF's found in source");
            }

            string dstPath;
            while (true)
            {
                if (!AskPath("destination", out dstPath, false, "{src}\\output"))
                    dstPath = Path.Combine(srcPath, "output");
                    break;
                if (!Path.GetFullPath(srcPath).Equals(Path.GetFullPath(dstPath), StringComparison.OrdinalIgnoreCase))
                    break;
                Console.WriteLine("- destination cannot be the same as the source");
            }

            Console.Write("Supply password: ");
            var pwd = Console.ReadLine();

            Directory.CreateDirectory(dstPath);
            Console.WriteLine($"Processing {srcPdfs.Length} PDF's:");
            foreach (var srcPdf in srcPdfs)
            {
                Console.WriteLine($"- {srcPdf}");
                try
                {
                    var dstPdf = Path.Combine(dstPath, Path.GetFileName(srcPdf));
                    DecryptFile(srcPdf, dstPdf, pwd);
                }
                catch (Exception E)
                {
                    Console.WriteLine($"# {E.GetType().Name}: {E.Message}");
                }
            }
        }
    }
}

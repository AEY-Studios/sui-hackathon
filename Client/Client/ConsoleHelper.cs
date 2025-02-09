namespace OllamaInstaller
{
    public static class ConsoleHelper
    {
        public static void WriteYellow(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteGreen(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteRed(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void DrawTextProgressBar(long progress, long total, int barSize)
        {
            double percentage = (double)progress / total;
            int filledBarSize = (int)(percentage * barSize);
            string bar = new string('=', filledBarSize) + new string(' ', barSize - filledBarSize);
            Console.Write($"\rDownloading [{bar}] {percentage * 100:0.00}%");
        }

        public static void DrawnLogo()
        {
            string[] AEY = new string[]
        {
            "        .XXXXXXXXXXXx;           .+XXXXXXXXXXXXXX+.    :XXXXXX             .+XXXXX: ",
            "        :&&&&&&&&&&&&&&X;     .x$&&&&&&&&&&&&&&&&&&$;  ;$&&&&$             .x&&&&&; ",
            "        :&&&&&&&&&&&&&&&&X.  ;$&&&&&&&&&&&&&&&&&&&&&&x.;$&&&&$             .x&&&&&; ",
            "        .xxxxxxxxxx$&&&&&&x.;$&&&&&&Xxxxxxxxxxx$&&&&&&+;$&&&&$             .x&&&&&; ",
            "                    :$&&&&$+x&&&&&x             ;&&&&&x+$&&&&$             .x&&&&&; ",
            "                     $&&&&$xx&&&&&:            .$&&&&&x+$&&&&$             .x&&&&&; ",
            "   .+$&&&&&&&&&&&&&&&&&&&&$xx&&&&&&&&&&&&&&&&&&&&&&&&$;;$&&&&&;            :$&&&&&; ",
            " .+&&&&&&&&&&&&&&&&&&&&&&&$xx&&&&&&&&&&&&&&&&&&&&&&&X: .X&&&&&&&$x.    .+$&&&&&&&X: ",
            ".+&&&&&&&&&&&&&&&&&&&&&&&&$xx&&&&&&&&&&&&&&&&&&&&&X:    .X&&&&&&&&&&$$&&&&&&&&&&X.  ",
            ":&&&&&&;             X&&&&$xx&&&&&:                       .x&&&&&&&&&&&&&&&&&$x.    ",
            ":&&&&&X.            :$&&&&$+x&&&&&x                           :X&&&&&&&&&&X;        ",
            ":X&&&&&&XXXXXXXXXXX&&&&&&&x ;$&&&&&&$XXXXXXXXX+                  :X&&&&&;           ",
            " :$&&&&&&&&&&&&&&&&&&&&&&X.  ;$&&&&&&&&&&&&&&&x                  .x&&&&&:           ",
            "  .x$&&&&&&&&&&&&&&&&&&$:     .+$&&&&&&&&&&&&&x                  .x&&&&&:           ",
            "     :+XXXXXXXXXXXXXx:           .+XX$XXXXXXXX+                  .+XXXXX:           "
        };
            Console.ForegroundColor = ConsoleColor.Cyan;
            foreach (string line in AEY)
            {
                Console.Write(line + "\n");
            }

            Console.ResetColor();
        }
    }
}

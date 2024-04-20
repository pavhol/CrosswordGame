using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Xml;
namespace crossword
{
    public enum CrosswordSize
    {
        Small
        , Normal
        , Large
        , VeryLarge
        , UsePrevious
    }


    class Crossword
    {
        int SizeX = 17;
        int SizeY = 17;

        string filename;
        XmlDocument doc = new XmlDocument();

        CrosswordSize size = CrosswordSize.Normal;

        private IBlock[,] blocks { get; set; }
        private List<Word> words = new List<Word>();
        Dictionary<string, string> initialWords = new Dictionary<string, string>() { };

        public Word[] GetWords()
        {
            return words.ToArray();
        }

        static public IBlock CreateBlock(char c)
        {
            if (c == '#')
            {
                return new BlackBlock();
            }
            return new CharacterBlock(c);
        }

        public IBlock[,] GetBlocks()
        {
            return blocks;
        }

        public bool IsSolved()
        {
            foreach (var block in blocks)
            {
                if (!block.IsCorrectAnswer())
                {
                    return false;
                }
            }
            return true;
        }


        private bool TryPlace(Word w, Point start, int minIntersections)
        {
            int inters = CountIntersections(w, start);

            // "Useless" condition for a special case. Handled seperately for clarity.
            if (inters < 0)
            {
                return false;
            }
            else if (inters < minIntersections)
            {
                return false;
            }
            else if (inters == w.GetCorrectWord().Length)
            {
                return false;
            }


            PlaceWord(w, start);
            return true;
        }

        // Helper function
        private Word GenerateWord(string wordName, string description, Random rstream)
        {
            return new Word(wordName, description, rstream.Next(2) == 0 ? Direction.Horizontal : Direction.Vertical);
        }

        public void GenerateNewCrossword(CrosswordSize newSize = CrosswordSize.UsePrevious)
        {
            if (newSize != CrosswordSize.UsePrevious)
            {
                size = newSize;
            }

            switch (size)
            {
                case CrosswordSize.Small:
                    SizeX = SizeY = 17;
                    break;
                case CrosswordSize.Normal:
                    SizeX = SizeY = 24;
                    break;
                case CrosswordSize.Large:
                    SizeX = SizeY = 31;
                    break;
                case CrosswordSize.VeryLarge:
                    SizeX = SizeY = 45;
                    break;
            }

            words.Clear();

            if (initialWords.Count < 4)
            {
                MessageBox.Show("Not enough words left for a new crossword. Reloading wordlist file.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                InitialiseFromFile();
            }

            const float GenerationComplexityFactor = 2.0f;
            blocks = new IBlock[SizeX, SizeY];


            int InitialSizeWords = initialWords.Count;

            Random r = new Random();

            DateTime timeStart = DateTime.Now;

            // Place first word at random
            bool Placed = false;
            while (!Placed)
            {
                int x = r.Next((int)Math.Ceiling(SizeX * 0.30), (int)Math.Floor(SizeX * 0.70));
                int y = r.Next((int)Math.Ceiling(SizeY * 0.30), (int)Math.Floor(SizeY * 0.70));

                int index = r.Next(initialWords.Count);
                string wordName = initialWords.Keys.ElementAt(index);

                Word w = GenerateWord(wordName, initialWords[wordName], r); // Get description using key

                if (TryPlace(w, new Point(x, y), 0))
                {
                    initialWords.Remove(wordName); // Remove by key
                    Placed = true;
                }
            }


            int LoopsWithoutProgress = 0;
            float GenerationFactor = InitialSizeWords * GenerationComplexityFactor;
            int SingleIntersectionCount = 0;
            while (initialWords.Count > 0 && LoopsWithoutProgress < GenerationFactor)
            {
                int wordsPlaced = InitialSizeWords - initialWords.Count;

                float wordsPlacedPercent = wordsPlaced / InitialSizeWords;

                int index = r.Next(initialWords.Count);
                string wordName = initialWords.Keys.ElementAt(index);

                Word w = GenerateWord(wordName, initialWords[wordName], r); // Get description using key


                int minIntersections = 1;

                if (wordsPlaced < 3)
                {
                    if (w.GetCorrectWord().Length > 5)
                    {
                        minIntersections = 1;
                    }
                    else
                    {
                        continue;
                    }

                }
                else if (LoopsWithoutProgress < GenerationFactor * 0.1)
                {
                    minIntersections = 4;
                }
                else if (LoopsWithoutProgress < GenerationFactor * 0.3)
                {
                    minIntersections = 3;
                }
                else if (LoopsWithoutProgress < GenerationFactor * 0.9)
                {
                    minIntersections = 2;
                }
                else
                {
                    minIntersections = 1;
                }

                LoopsWithoutProgress++;
                if (TryPlaceEverywhere(w, minIntersections, r))
                {
                    if (minIntersections == 1)
                    {
                        SingleIntersectionCount++;
                    }
                    LoopsWithoutProgress = 0;
                    initialWords.Remove(wordName); // Remove by key
                }
            }


            TimeSpan ts = DateTime.Now - timeStart;

            int ms = (int)ts.TotalMilliseconds;

            if (ms > 3000)
            {
                MessageBox.Show("Took " + ms + "ms to make this crossword.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


            // Fill empty blocks
            for (int row = 0; row < blocks.GetLength(0); row++)
            {
                for (int col = 0; col < blocks.GetLength(1); col++)
                {
                    if (blocks[row, col] == null)
                    {
                        blocks[row, col] = new BlackBlock();
                    }
                }
            }
        }


        private bool TryPlaceEverywhere(Word word, int minIntersections, Random rstream)
        {
            int offseti = rstream.Next(SizeX);
            int offsetj = rstream.Next(SizeY);

            for (int i = 0; i < SizeX - 1; i++)
            {
                for (int j = 0; j < SizeY - 1; j++)
                {
                    Point p = new Point(
                            (i + offseti) % (SizeX - 2) + 1,
                            (j + offsetj) % (SizeY - 2) + 1
                        );

                    if (TryPlace(word, p, minIntersections))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        private bool IsValidPoint(Point p)
        {
            return IsValidPoint(p.X, p.Y);
        }

        private bool IsValidPoint(int x, int y)
        {
            return x >= 0 && y >= 0
                && x < blocks.GetLength(0)
                && y < blocks.GetLength(1);
        }

        // Helper function to resolve points cleaner. 
        // This will also work for negative indexes
        private static Point GetWordCoord(Word word, Point startPoint, int index, int parallelOffset = 0)
        {
            startPoint.X += word.GetDirection() == Direction.Vertical ? parallelOffset : 0;
            startPoint.Y += word.GetDirection() == Direction.Horizontal ? parallelOffset : 0;

            int x = startPoint.X + (word.GetDirection() == Direction.Horizontal ? index : 0);
            int y = startPoint.Y + (word.GetDirection() == Direction.Vertical ? index : 0);
            return new Point(x, y);
        }

        private void ProhibitOverwrite(Point p, Direction dir)
        {
            if (blocks[p.X, p.Y] == null)
            {
                blocks[p.X, p.Y] = new BlackBlock(dir == Direction.Horizontal ? BlockOverwrite.VerticalOnly : BlockOverwrite.HorizontalOnly);
            }
            else
            {
                blocks[p.X, p.Y].RemoveOverwritePossibility(dir);
            }
        }

        // Excpects a word that can be placed
        private void PlaceWord(Word word, Point start)
        {
            Point before = GetWordCoord(word, start, -1);
            Point after = GetWordCoord(word, start, word.GetLength());

            // Its ok to overwrite the old block here since black blocks do not get stored anywhere else than the block[,] array
            blocks[before.X, before.Y] = new BlackBlock(BlockOverwrite.None);
            blocks[after.X, after.Y] = new BlackBlock(BlockOverwrite.None);


            for (int i = 0; i < word.GetLength(); i++)
            {
                Point p = GetWordCoord(word, start, i);
                if (blocks[p.X, p.Y] == null || blocks[p.X, p.Y] is BlackBlock)
                {
                    blocks[p.X, p.Y] = new CharacterBlock(word.GetCorrectWord()[i]);
                }
                word.SetSharedBlock(blocks[p.X, p.Y] as CharacterBlock, i);

                ProhibitOverwrite(GetWordCoord(word, start, i, 1), word.GetDirection());
                ProhibitOverwrite(GetWordCoord(word, start, i, -1), word.GetDirection());
            }
            words.Add(word);
        }

        private bool CanWordFit(Word word, Point start)
        {
            // Check lower bound
            if (start.X <= 0 || start.Y <= 0)
            {
                return false;
            }

            // Check clearence before (previous must be black point)
            Point before = GetWordCoord(word, start, -1);
            if (blocks[before.X, before.Y] != null && blocks[before.X, before.Y].GetAnswer() != '#')
            {
                return false;
            }

            Point after = GetWordCoord(word, start, word.GetLength());

            // Check upper bound
            if (after.X >= blocks.GetLength(0) || after.Y >= blocks.GetLength(1))
            {
                return false;
            }

            // Check clearence after.
            if (blocks[after.X, after.Y] != null && blocks[after.X, after.Y].GetAnswer() != '#')
            {
                return false;
            }


            return true;
        }

        // Will return -1 if the word cannot be placed.
        private int CountIntersections(Word word, Point start)
        {
            int intersections = 0;

            if (!CanWordFit(word, start))
            {
                return -1;
            }

            for (int i = 0; i < word.GetLength(); ++i)
            {
                Point p = GetWordCoord(word, start, i);

                if (blocks[p.X, p.Y] != null)
                {
                    if (blocks[p.X, p.Y].GetAnswer() == word.GetCorrectWord()[i])
                    {
                        intersections++;
                    }
                    else if (!blocks[p.X, p.Y].CanOverwrite(word.GetDirection()))
                    {
                        return -1;
                    }
                }
            }
            return intersections;
        }
        public static Dictionary<string, string> ReadWordsFromXml(string xmlFilePath)
        {
            var words = new Dictionary<string, string>();

            using (var xmlReader = XmlReader.Create(xmlFilePath))
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "Word")
                    {
                        var wordName = xmlReader["name"];
                        var wordDescription = xmlReader["description"];

                        words.Add(wordName, wordDescription);
                    }
                }
            }

            return words;
        }
        public bool InitialiseFromFile()
        {
            initialWords.Clear();
            try
            {
                doc.Load(filename);
                var wordNodes = doc.SelectNodes("//Word");
                foreach (XmlNode wordNode in wordNodes)
                {
                    var wordName = wordNode.Attributes["name"].Value;
                    var wordDescription = wordNode.Attributes["description"].Value;
                    initialWords.Add(wordName, wordDescription);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading word file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool OpenWordFile()
        {
            string currentDir = System.Environment.CurrentDirectory;

            if (File.Exists(Path.Combine(currentDir, "WordList.xml")))
                filename = Path.Combine(currentDir, "WordList.xml");

            return InitialiseFromFile();
        }
    }
}

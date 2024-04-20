using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace crossword
{
    // уровень сложности
    public enum CrosswordSize
    {
        Small,
        Normal,
        Large
    }

    class Crossword
    {
        // размер поля
        private int _xsz;
        private int _ysz;

        CrosswordSize _size;
        public IBlock[,] blocks { get; private set; }
        Dictionary<string, string> _word_list;
        public List<Word> words { get; private set; }

        public Crossword()
        {
            _word_list = new Dictionary<string, string>();
            words = new List<Word>();
        }

        // создание блока
        static public IBlock CreateBlock(char c)
        {
            if (c == '#')
                return new BlackBlock();

            return new CharacterBlock(c);
        }

        // проверяем, решен ли кроссворд
        public bool IsSolved()
        {
            foreach (var block in blocks)
            {
                if (!block.IsCorrectAnswer())
                    return false;
            }

            return true;
        }

        // проверяем пересечения, размещаем слово
        private bool TryPlace(Word w, Point start, int minIntersections)
        {
            int inters = CountIntersections(w, start);
            if (inters < 0 || inters < minIntersections || inters == w.GetCorrectWord().Length)
                return false;

            PlaceWord(w, start);
            return true;
        }

        // генерируем слово с случайным направлением
        private Word GenerateWord(string wordName, string description, Random rstream)
        {
            return new Word(wordName, description, rstream.Next(2) == 0 ? Direction.Horizontal : Direction.Vertical);
        }

        // генерация кроссворда
        public void GenerateNewCrossword(Dictionary<string, string> word_list, CrosswordSize size)
        {
            _word_list = new Dictionary<string, string>(word_list);
            _size = size;
            switch (_size)
            {
                case CrosswordSize.Small:
                    _xsz = _ysz = 17;
                    break;
                case CrosswordSize.Normal:
                    _xsz = _ysz = 24;
                    break;
                case CrosswordSize.Large:
                    _xsz = _ysz = 31;
                    break;
            }

            words.Clear();
            blocks = new IBlock[_xsz, _ysz];
            if (_word_list.Count < 4)
            {
                MessageBox.Show("Слишком мало слов в словаре для составления кроссворда.");
                return;
            }

            Random rand = new Random();            
            while (true)
            {
                // положение первого слова в пределах 30 - 70 % от размеров поля
                int x = rand.Next((int)Math.Ceiling(_xsz * 0.30), (int)Math.Floor(_xsz * 0.70));
                int y = rand.Next((int)Math.Ceiling(_ysz * 0.30), (int)Math.Floor(_ysz * 0.70));

                string wordName = _word_list.Keys.ElementAt(rand.Next(_word_list.Count));
                Word w = GenerateWord(wordName, _word_list[wordName], rand);
                if (TryPlace(w, new Point(x, y), 0))
                {
                    _word_list.Remove(wordName);
                    break;
                }
            }

            int word_count = _word_list.Count;
            int without_progress = 0;
            float generator_factor = word_count * 2.0f;
            while (_word_list.Count > 0 && without_progress < generator_factor)
            {
                int words_placed = word_count - _word_list.Count;
                int index = rand.Next(_word_list.Count);
                string word_key = _word_list.Keys.ElementAt(index);
                Word w = GenerateWord(word_key, _word_list[word_key], rand);

                int minIntersections;
                if (words_placed < 3)
                {
                    if (w.GetCorrectWord().Length > 5)
                        minIntersections = 1;
                    else
                        continue;
                }
                else if (without_progress < generator_factor * 0.1)
                    minIntersections = 4;
                else if (without_progress < generator_factor * 0.3)
                    minIntersections = 3;
                else if (without_progress < generator_factor * 0.9)
                    minIntersections = 2;
                else
                    minIntersections = 1;

                if (TryPlaceEverywhere(w, minIntersections, rand))
                {
                    without_progress = 0;
                    _word_list.Remove(word_key);
                }
                else
                    without_progress++;
            }

            // дозаполняем все пустые клетки черными блоками с любым направлением
            for (int row = 0; row < blocks.GetLength(0); row++)
            {
                for (int col = 0; col < blocks.GetLength(1); col++)
                {
                    if (blocks[row, col] == null)
                        blocks[row, col] = new BlackBlock();
                }
            }
        }

        // пробуем разместить слово по всем позициям
        private bool TryPlaceEverywhere(Word word, int minIntersections, Random rstream)
        {
            int offseti = rstream.Next(_xsz);
            int offsetj = rstream.Next(_ysz);

            for (int i = 0; i < _xsz - 1; i++)
            {
                for (int j = 0; j < _ysz - 1; j++)
                {
                    Point p = new Point(
                            (i + offseti) % (_xsz - 2) + 1,
                            (j + offsetj) % (_ysz - 2) + 1
                        );

                    if (TryPlace(word, p, minIntersections))
                        return true;
                }
            }

            return false;
        }

        // получаем координаты позиции слова
        private static Point GetWordCoord(Word word, Point startPoint, int index, int parallelOffset = 0)
        {
            startPoint.X += word.GetDirection() == Direction.Vertical ? parallelOffset : 0;
            startPoint.Y += word.GetDirection() == Direction.Horizontal ? parallelOffset : 0;

            int x = startPoint.X + (word.GetDirection() == Direction.Horizontal ? index : 0);
            int y = startPoint.Y + (word.GetDirection() == Direction.Vertical ? index : 0);
            return new Point(x, y);
        }

        // ставим соседним точкам, что можно записывать только в противоположном направлении
        private void ProhibitOverwrite(Point p, Direction dir)
        {
            if (blocks[p.X, p.Y] == null)
                blocks[p.X, p.Y] = new BlackBlock(dir == Direction.Horizontal ? BlockOverwrite.VerticalOnly : BlockOverwrite.HorizontalOnly);
            else
                blocks[p.X, p.Y].RemoveOverwritePossibility(dir);
        }

        // Размещение слова, блокирование соседних ячеек
        private void PlaceWord(Word word, Point start)
        {
            Point before = GetWordCoord(word, start, -1);
            Point after = GetWordCoord(word, start, word.GetLength());

            blocks[before.X, before.Y] = new BlackBlock(BlockOverwrite.None);
            blocks[after.X, after.Y] = new BlackBlock(BlockOverwrite.None);

            for (int i = 0; i < word.GetLength(); i++)
            {
                Point p = GetWordCoord(word, start, i);
                if (blocks[p.X, p.Y] == null || blocks[p.X, p.Y] is BlackBlock)
                    blocks[p.X, p.Y] = new CharacterBlock(word.GetCorrectWord()[i]);

                word.SetSharedBlock(blocks[p.X, p.Y] as CharacterBlock, i);
                ProhibitOverwrite(GetWordCoord(word, start, i, 1), word.GetDirection());
                ProhibitOverwrite(GetWordCoord(word, start, i, -1), word.GetDirection());
            }

            words.Add(word);
        }

        // проверяем, можно ли разместить слово
        private bool CanWordFit(Word word, Point start)
        {
            if (start.X <= 0 || start.Y <= 0)
                return false;

            Point before = GetWordCoord(word, start, -1);
            if (blocks[before.X, before.Y] != null && blocks[before.X, before.Y].GetAnswer() != '#')
                return false;

            Point after = GetWordCoord(word, start, word.GetLength());
            if (after.X >= blocks.GetLength(0) || after.Y >= blocks.GetLength(1))
                return false;

            if (blocks[after.X, after.Y] != null && blocks[after.X, after.Y].GetAnswer() != '#')
                return false;

            return true;
        }

        // считаем пересечения
        private int CountIntersections(Word word, Point start)
        {
            if (!CanWordFit(word, start))
                return -1;

            int intersections = 0;
            for (int i = 0; i < word.GetLength(); ++i)
            {
                Point p = GetWordCoord(word, start, i);
                if (blocks[p.X, p.Y] != null)
                {
                    if (blocks[p.X, p.Y].GetAnswer() == word.GetCorrectWord()[i])
                        intersections++;
                    else if (!blocks[p.X, p.Y].CanOverwrite(word.GetDirection()))
                        return -1;
                }
            }
        }
    }
}

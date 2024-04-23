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
        public IBlock[,] _blocks { get; private set; }
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
            foreach (var block in _blocks)
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

        // восстановление кроссворда
        public void ContinueCrossword(Dictionary<string, string> word_list, IBlock[,] blocks, List<string> lstwords, List<string> directions, List<Point> startpoints)
        {
            // убираем недозаполненные слова
            for (int i = 0; i < lstwords.Count; ++i)
            {
                for (int j = 0; j < lstwords[i].Length; ++j)
                {
                    if (char.IsLower(lstwords[i][j]))
                    {
                        lstwords[i] = lstwords[i].ToLower();
                        break;
                    }
                }
            }

            _word_list = new Dictionary<string, string>(word_list);
            switch (blocks.GetLength(0))
            {
                case 17:
                    _size = CrosswordSize.Small;
                    break;
                case 24:
                    _size = CrosswordSize.Normal;
                    break;
                case 31:
                    _size = CrosswordSize.Large;
                    break;
            }

            _xsz = _ysz = blocks.GetLength(0);
            _blocks = new IBlock[_xsz, _ysz];
            words.Clear();
            for (int i = 0; i < lstwords.Count; i++)
            {
                Direction direction = (directions[i] != "Horizontal" ? Direction.Horizontal : Direction.Vertical);
                Word word = new Word(lstwords[i].ToLower(), _word_list[lstwords[i].ToLower()], direction);
                PlaceWord(word, startpoints[i]);
            }

            // дозаполняем все пустые клетки черными блоками с любым направлением
            for (int row = 0; row < _blocks.GetLength(0); row++)
            {
                for (int col = 0; col < _blocks.GetLength(1); col++)
                {
                    if (_blocks[row, col] == null)
                        _blocks[row, col] = new BlackBlock();
                }
            }
        }

        public void UpdateSolved(List<string> lstwords, List<Point> startpoints)
        {
            for (int i = 0; i < lstwords.Count; i++)
            {
                for (int j = 0; j < lstwords[i].Length; ++j)
                {
                    if (char.IsUpper(lstwords[i][j]))
                    {
                        Point sym_point = GetWordCoord(words[i], startpoints[i], j);
                        ((CharacterBlock)_blocks[sym_point.X, sym_point.Y]).SetSolved();
                    }
                }
            }
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
            _blocks = new IBlock[_xsz, _ysz];
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
            for (int row = 0; row < _blocks.GetLength(0); row++)
            {
                for (int col = 0; col < _blocks.GetLength(1); col++)
                {
                    if (_blocks[row, col] == null)
                        _blocks[row, col] = new BlackBlock();
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
            if (_blocks[p.X, p.Y] == null)
                _blocks[p.X, p.Y] = new BlackBlock(dir == Direction.Horizontal ? BlockOverwrite.VerticalOnly : BlockOverwrite.HorizontalOnly);
            else
                _blocks[p.X, p.Y].RemoveOverwritePossibility(dir);
        }

        // Размещение слова, блокирование соседних ячеек
        private void PlaceWord(Word word, Point start)
        {
            Point before = GetWordCoord(word, start, -1);
            Point after = GetWordCoord(word, start, word.GetLength());

            _blocks[before.X, before.Y] = new BlackBlock(BlockOverwrite.None);
            _blocks[after.X, after.Y] = new BlackBlock(BlockOverwrite.None);

            for (int i = 0; i < word.GetLength(); i++)
            {
                Point p = GetWordCoord(word, start, i);
                if (_blocks[p.X, p.Y] == null || _blocks[p.X, p.Y] is BlackBlock)
                    _blocks[p.X, p.Y] = new CharacterBlock(word.GetCorrectWord()[i]);

                word.SetSharedBlock(_blocks[p.X, p.Y] as CharacterBlock, i);
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
            if (_blocks[before.X, before.Y] != null && _blocks[before.X, before.Y].GetAnswer() != '#')
                return false;

            Point after = GetWordCoord(word, start, word.GetLength());
            if (after.X >= _blocks.GetLength(0) || after.Y >= _blocks.GetLength(1))
                return false;

            if (_blocks[after.X, after.Y] != null && _blocks[after.X, after.Y].GetAnswer() != '#')
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
                if (_blocks[p.X, p.Y] != null)
                {
                    if (_blocks[p.X, p.Y].GetAnswer() == word.GetCorrectWord()[i])
                        intersections++;
                    else if (!_blocks[p.X, p.Y].CanOverwrite(word.GetDirection()))
                        return -1;
                }
            }

            return intersections;
        }
    }
}

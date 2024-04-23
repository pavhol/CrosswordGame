using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace crossword_v1
{
    public enum Direction
    {
        Horizontal,
        Vertical
    }

    public class Word
    {
        Direction _direction;
        string _correct_word;
        string _description;
        CharacterBlock[] _blocks;
        bool finished = false;

        public delegate void OnFinish(Word sender);
        public OnFinish onFinish;

        public Word(string correctWord, string description, Direction direction = Direction.Horizontal)
        {
            this._correct_word = correctWord.ToUpper();
            this._direction = direction;
            this._description = description;
            this._blocks = new CharacterBlock[correctWord.Length];
        }

        public void OnDescriptionClicked()
        {
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            MessageBox.Show(_correct_word, _description, buttons);
        }

        public void OnBlockUpdated(CharacterBlock block)
        {
            if (finished)
            {
                return;
            }

            if (IsFilled())
            {
                finished = TryConfirm();
                if (finished)
                {
                    onFinish.Invoke(this);
                }
            }
            else if (this == MainWindow.selectedWord)
            {
                int currentIndex = Array.FindIndex(_blocks, p => p == block) + 1;

                if (_blocks.Length > currentIndex)
                {
                    _blocks[currentIndex].Focus();
                }
            }
        }

        public void SetSharedBlock(CharacterBlock block, int position)
        {
            switch (_direction)
            {
                case Direction.Horizontal:
                    block.SetHorizontalWord(this);
                    break;
                case Direction.Vertical:
                    block.SetVerticalWord(this);
                    break;
                default:
                    throw new Exception("Unhandled case for enum WordDirection.");
            }
            _blocks.SetValue(block, position);
        }

        // This will only generate the block if not already set.
        public CharacterBlock GenerateBlock(int position)
        {
            var block = _blocks[position];
            if (block == null)
            {
                block = new CharacterBlock(_correct_word[position]);
                _blocks[position] = block;
            }
            return block;
        }

        // Generate the remaining unset blocks. (or all blocks if none are already generated/set)
        public void GenerateRemainingBlocks()
        {
            for (int i = 0; i < _blocks.Length; i++)
            {
                GenerateBlock(i);
            }
        }

        internal void BackspaceBefore(CharacterBlock characterBlock)
        {
            if (finished)
            {
                return;
            }

            int currentIndex = Array.FindIndex(_blocks, p => p == characterBlock) - 1;
            if (currentIndex >= 0)
            {
                _blocks[currentIndex].Backspace();
            }
        }

        public int GetLength()
        {
            return _correct_word.Length;
        }

        public void Select()
        {
            _blocks[0].Focus();
            foreach (var block in _blocks)
            {
                block.Highlight();
            }
        }

        public void DeSelect()
        {
            foreach (var block in _blocks)
            {
                block.DeSelect();
            }
        }

        public Direction GetDirection()
        {
            return _direction;
        }

        public CharacterBlock GetBlockAt(int position)
        {
            return _blocks[position];
        }

        public string GetCorrectWord()
        {
            return _correct_word;
        }

        public string GetDescription()
        {
            return _description;
        }

        // returns true when the all blocks contain correct answers
        public bool IsCorrect()
        {
            foreach (var block in _blocks)
            {
                if (!block.IsCorrectAnswer())
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsFilled()
        {
            foreach (var block in _blocks)
            {
                if (!block.IsSet())
                {
                    return false;
                }
            }
            return true;
        }

        // returns true when the all blocks are confirmed
        public bool IsFinished()
        {
            return finished;
        }

        public bool TryConfirm()
        {
            bool correct = IsCorrect();

            foreach (var block in _blocks)
            {
                if (correct)
                {
                    block.SetConfirmed();
                }
                else
                {
                    block.SetWrong();
                }
            }
            return correct;
        }
        override public string ToString()
        {
            return _description;
        }
    }


}

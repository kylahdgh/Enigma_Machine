using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Net.NetworkInformation;

namespace Enigma_Machine
{
    public partial class MainWindow : Window
    {
        string _control = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string _ring1 = "DMTWSILRUYQNKFEJCAZBPGXOHV";
        string _ring2 = "HQZGPJTMOBLNCIFDYAWVEUSRKX";
        string _ring3 = "UQNTLSZFMREHDPXKIBVYGJCWOA";
        string _reflector = "YRUHQSLDPXNGOKMIEBFZCWVJAT";

        int[] _keyOffset = { 0, 0, 0 };
        int[] _initOffset = { 0, 0, 0 };

        bool _rotor = false;

        Dictionary<char, char> _plugboard = new Dictionary<char, char>();
        private bool _plugboardSet = false;

        public MainWindow()
        {
            InitializeComponent();

            SetDefaults();

            _rotor = false;
            btnRotor.Content = "Rotor On";
            btnRotor.IsEnabled = false;
        }

        private void DisplayRing(Label displayLabel, string ring)
        {
            string temp = "";
            foreach (char r in ring)
                temp += r + "\t";
            displayLabel.Content = temp;
        }

        private int IndexSearch(string ring, char letter)
        {
            int index = 0;
            for (int x = 0; x < ring.Length; x++)
            {
                if (ring[x] == letter)
                {
                    index = x;
                    break;
                }
            }
            return index;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.ToString().Length == 1 && lblInput.Content.ToString().Length < 128)
            {
                if ((int)e.Key.ToString()[0] >= 65 && (int)e.Key.ToString()[0] <= 90)
                {
                    lblInput.Content += e.Key.ToString();
                    lblEncrypt.Content += Encrypt(e.Key.ToString()[0]) + "";
                    lblEncryptMirror.Content += Mirror(e.Key.ToString()[0]) + "";

                    if (_rotor)
                    {
                        Rotate(true);
                        DisplayRing(lblRing1, _ring1);
                        DisplayRing(lblRing2, _ring2);
                        DisplayRing(lblRing3, _ring3);
                    }
                }
            }
            else if (e.Key == Key.Space)
            {
                lblInput.Content += " ";
                lblEncrypt.Content += " ";
                lblEncryptMirror.Content += " ";
            }
            else if (e.Key == Key.Back)
            {
                Rotate(false);
                DisplayRing(lblRing1, _ring1);
                DisplayRing(lblRing2, _ring2);
                DisplayRing(lblRing3, _ring3);

                lblInput.Content = RemoveLastLetter(lblInput.Content.ToString());
                lblEncrypt.Content = RemoveLastLetter(lblEncrypt.Content.ToString());
                lblEncryptMirror.Content = RemoveLastLetter(lblEncryptMirror.Content.ToString());
            }
        }

        private char Encrypt(char letter)
        {
            char newChar = letter;

            if (_plugboard.ContainsKey(newChar))
                newChar = _plugboard[newChar];
            else if (_plugboard.ContainsValue(newChar))
                newChar = _plugboard.FirstOrDefault(x => x.Value == newChar).Key;

            newChar = _ring1[IndexSearch(_control, newChar)];
            newChar = _ring2[IndexSearch(_control, newChar)];
            newChar = _ring3[IndexSearch(_control, newChar)];

            newChar = _reflector[IndexSearch(_control, newChar)];

            newChar = _control[IndexSearch(_ring3, newChar)];
            newChar = _control[IndexSearch(_ring2, newChar)];
            newChar = _control[IndexSearch(_ring1, newChar)];

            if (_plugboard.ContainsKey(newChar))
                newChar = _plugboard[newChar];
            else if (_plugboard.ContainsValue(newChar))
                newChar = _plugboard.FirstOrDefault(x => x.Value == newChar).Key;

            return newChar;
        }

        private char Mirror(char letter)
        {
            char newChar = Encrypt(letter);

            newChar = _control[IndexSearch(_ring3, newChar)];
            newChar = _control[IndexSearch(_ring2, newChar)];
            newChar = _control[IndexSearch(_ring1, newChar)];

            return newChar;
        }

        private void SetDefaults()
        {
            _control = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            _ring1 = "DMTWSILRUYQNKFEJCAZBPGXOHV";
            _ring2 = "HQZGPJTMOBLNCIFDYAWVEUSRKX";
            _ring3 = "UQNTLSZFMREHDPXKIBVYGJCWOA";
            _keyOffset = new int[] { 0, 0, 0 };

            lblInput.Content = "";
            lblEncrypt.Content = "";
            lblEncryptMirror.Content = "";

            DisplayRing(lblControlRing, _control);
            DisplayRing(lblRing1, _ring1);
            DisplayRing(lblRing2, _ring2);
            DisplayRing(lblRing3, _ring3);
        }

        private void Rotate(bool forward)
        {
            if (forward)
            {
                _keyOffset[0]++;
                _ring1 = MoveValues(forward, _ring1);

                if (_keyOffset[0] / _control.Length >= 1)
                {
                    _keyOffset[0] = 0;
                    _keyOffset[1]++;
                    _ring2 = MoveValues(forward, _ring2);
                    if (_keyOffset[1] / _control.Length >= 1)
                    {
                        _keyOffset[1] = 0;
                        _keyOffset[2]++;
                        _ring3 = MoveValues(forward, _ring3);
                    }
                }
            }
            else
            {
                if (_keyOffset[0] > 0 || _keyOffset[1] > 0)
                {
                    _keyOffset[0]--;
                    _ring1 = MoveValues(forward, _ring1);
                    if (_keyOffset[0] < 0)
                    {
                        _keyOffset[0] = 25;
                        _keyOffset[1]--;
                        _ring2 = MoveValues(forward, _ring2);
                        if (_keyOffset[1] < 0)
                        {
                            _keyOffset[1] = 25;
                            _keyOffset[2]--;
                            _ring3 = MoveValues(forward, _ring3);
                            if (_keyOffset[2] < 0)
                                _keyOffset[2] = 25;
                        }
                    }
                }
            }

            DisplayOffset();
        }

        private string MoveValues(bool forward, string ring)
        {
            char movingValue = ' ';
            string newRing = "";

            if (forward)
            {
                movingValue = ring[0];
                for (int x = 1; x < ring.Length; x++)
                    newRing += ring[x];
                newRing += movingValue;
            }
            else
            {
                movingValue = ring[ring.Length - 1];
                for (int x = 0; x < ring.Length - 1; x++)
                    newRing += ring[x];
                newRing = movingValue + newRing;
            }

            return newRing;
        }

        private void btnRotor_Click(object sender, RoutedEventArgs e)
        {
            SetDefaults();

            if (int.TryParse(txtBRing1Init.Text, out _initOffset[0]) &&
                int.TryParse(txtBRing2Init.Text, out _initOffset[1]) &&
                int.TryParse(txtBRing3Init.Text, out _initOffset[2]))
            {
                if (_initOffset[0] >= 0 && _initOffset[0] <= 25 &&
                    _initOffset[1] >= 0 && _initOffset[1] <= 25 &&
                    _initOffset[2] >= 0 && _initOffset[2] <= 25)
                {
                    txtBRing1Init.IsEnabled = false;
                    txtBRing2Init.IsEnabled = false;
                    txtBRing3Init.IsEnabled = false;

                    _rotor = true;
                    btnRotor.Content = "Settings Lock";

                    _ring1 = InitializeRotors(_initOffset[0], _ring1);
                    _ring2 = InitializeRotors(_initOffset[1], _ring2);
                    _ring3 = InitializeRotors(_initOffset[2], _ring3);

                    DisplayRing(lblRing1, _ring1);
                    DisplayRing(lblRing2, _ring2);
                    DisplayRing(lblRing3, _ring3);
                    DisplayOffset();
                }
            }
        }

        private string InitializeRotors(int initial, string ring)
        {
            string newRing = ring;
            for (int x = 0; x < initial; x++)
                newRing = MoveValues(true, newRing);
            return newRing;
        }

        private string RemoveLastLetter(string word)
        {
            string newWord = "";
            for (int x = 0; x < word.Length - 1; x++)
                newWord += word[x];
            return newWord;
        }

        private void txtBRing1Init_GotFocus(object sender, RoutedEventArgs e)
        {
            txtBRing1Init.Text = "";
        }

        private void txtBRing2Init_GotFocus(object sender, RoutedEventArgs e)
        {
            txtBRing2Init.Text = "";
        }

        private void txtBRing3Init_GotFocus(object sender, RoutedEventArgs e)
        {
            txtBRing3Init.Text = "";
        }

        private void DisplayOffset()
        {
            txtBRing1Init.Text = _keyOffset[0] + "";
            txtBRing2Init.Text = _keyOffset[1] + "";
            txtBRing3Init.Text = _keyOffset[2] + "";
        }

        private void SetupPlugboard(string plugboardSettings)
        {
            _plugboard.Clear();
            string[] pairs = plugboardSettings.ToUpper().Split(' ');
            foreach (string pair in pairs)
            {
                if (pair.Length == 2)
                {
                    _plugboard[pair[0]] = pair[1];
                    _plugboard[pair[1]] = pair[0];
                }
            }
        }

        private void btnSetPlugboard_Click(object sender, RoutedEventArgs e)
        {
            if (_plugboardSet)
            {
                MessageBox.Show("Plugboard is already set.");
                return;
            }

            SetupPlugboard(txtPlugboard.Text);
            _plugboardSet = true;
            btnRotor.IsEnabled = true;

            if (_plugboardSet)
            {
                txtPlugboard.IsEnabled = false;
            }
        }

        private void txtPlugboard_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblInput.Content = "";
            lblEncrypt.Content = "";
            lblEncryptMirror.Content = "";
        }
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;

namespace PROG7312_POE.Extensions
{
    public class PlaceholderTextBox : TextBox
    {
        private string _placeholderText = string.Empty;
        private bool _isPlaceholder = true;
        private Color _originalForeColor;
        private Color _placeholderColor = SystemColors.GrayText;

        public string PlaceholderText
        {
            get => _placeholderText;
            set
            {
                _placeholderText = value;
                if (string.IsNullOrEmpty(this.Text) || this.Text == _placeholderText)
                {
                    SetPlaceholder();
                }
            }
        }

        public Color PlaceholderColor
        {
            get => _placeholderColor;
            set
            {
                _placeholderColor = value;
                if (_isPlaceholder)
                {
                    this.ForeColor = _placeholderColor;
                }
            }
        }

        public PlaceholderTextBox()
        {
            _originalForeColor = this.ForeColor;
            this.Enter += RemovePlaceholder;
            this.Leave += SetPlaceholderIfEmpty;
        }

        private void SetPlaceholder()
        {
            if (string.IsNullOrEmpty(this.Text) || this.Text == _placeholderText)
            {
                _isPlaceholder = true;
                this.Text = _placeholderText;
                this.ForeColor = _placeholderColor;
            }
        }

        private void RemovePlaceholder(object sender, EventArgs e)
        {
            if (_isPlaceholder)
            {
                _isPlaceholder = false;
                this.Text = string.Empty;
                this.ForeColor = _originalForeColor;
            }
        }

        private void SetPlaceholderIfEmpty(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.Text) || this.Text == _placeholderText)
            {
                SetPlaceholder();
            }
        }
    }
}

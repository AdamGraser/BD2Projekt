﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lekarz
{
    /// <summary>
    /// Logika interakcji dla klasy AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        /// <summary>
        /// Konstruktor.
        /// </summary>
        public AboutDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "OK".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

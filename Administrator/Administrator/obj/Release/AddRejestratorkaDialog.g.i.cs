﻿#pragma checksum "..\..\AddRejestratorkaDialog.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "12B4C5182E4F4DCBE0C9EA966D76BEED"
//------------------------------------------------------------------------------
// <auto-generated>
//     Ten kod został wygenerowany przez narzędzie.
//     Wersja wykonawcza:4.0.30319.18449
//
//     Zmiany w tym pliku mogą spowodować nieprawidłowe zachowanie i zostaną utracone, jeśli
//     kod zostanie ponownie wygenerowany.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Administrator {
    
    
    /// <summary>
    /// AddRejestratorkaDialog
    /// </summary>
    public partial class AddRejestratorkaDialog : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 22 "..\..\AddRejestratorkaDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox loginTextBox;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\AddRejestratorkaDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.PasswordBox pwdTextBox;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\AddRejestratorkaDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DatePicker DeactivateBox;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\AddRejestratorkaDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox imieTextBox;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\AddRejestratorkaDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox nazwiskoTextBox;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\AddRejestratorkaDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button okButton;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\AddRejestratorkaDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button cancelButton;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Administrator;component/addrejestratorkadialog.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\AddRejestratorkaDialog.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.loginTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 22 "..\..\AddRejestratorkaDialog.xaml"
            this.loginTextBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.BoxChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.pwdTextBox = ((System.Windows.Controls.PasswordBox)(target));
            
            #line 25 "..\..\AddRejestratorkaDialog.xaml"
            this.pwdTextBox.PasswordChanged += new System.Windows.RoutedEventHandler(this.BoxChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.DeactivateBox = ((System.Windows.Controls.DatePicker)(target));
            return;
            case 4:
            this.imieTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 31 "..\..\AddRejestratorkaDialog.xaml"
            this.imieTextBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.BoxChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.nazwiskoTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 34 "..\..\AddRejestratorkaDialog.xaml"
            this.nazwiskoTextBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.BoxChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.okButton = ((System.Windows.Controls.Button)(target));
            
            #line 37 "..\..\AddRejestratorkaDialog.xaml"
            this.okButton.Click += new System.Windows.RoutedEventHandler(this.okButton_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.cancelButton = ((System.Windows.Controls.Button)(target));
            
            #line 38 "..\..\AddRejestratorkaDialog.xaml"
            this.cancelButton.Click += new System.Windows.RoutedEventHandler(this.cancelButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}


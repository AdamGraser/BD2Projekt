﻿#pragma checksum "..\..\MainWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "B92D867C783F148548ED9C7617CA7E54"
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
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.Chromes;
using Xceed.Wpf.Toolkit.Core.Converters;
using Xceed.Wpf.Toolkit.Core.Input;
using Xceed.Wpf.Toolkit.Core.Media;
using Xceed.Wpf.Toolkit.Core.Utilities;
using Xceed.Wpf.Toolkit.Panels;
using Xceed.Wpf.Toolkit.Primitives;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Commands;
using Xceed.Wpf.Toolkit.PropertyGrid.Converters;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using Xceed.Wpf.Toolkit.Zoombox;


namespace Rejestratorka {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 8 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem logoutMenuItem;
        
        #line default
        #line hidden
        
        
        #line 10 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem aboutMenuItem;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox patientNameTextBox1;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox patientSurnameTextBox1;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Xceed.Wpf.Toolkit.MaskedTextBox peselTextBox;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button findPatientButton;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button clearFilterButton1;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button addPatientButton;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid patientsDataGrid;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button registerVisitButton;
        
        #line default
        #line hidden
        
        
        #line 51 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ScrollViewer TabRejScrollViewer;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox doctorsList;
        
        #line default
        #line hidden
        
        
        #line 57 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox hoursOfVisitsList;
        
        #line default
        #line hidden
        
        
        #line 62 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DatePicker visitDate;
        
        #line default
        #line hidden
        
        
        #line 66 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Xceed.Wpf.Toolkit.TimePicker visitTime;
        
        #line default
        #line hidden
        
        
        #line 74 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TabItem registeredVisitsTab;
        
        #line default
        #line hidden
        
        
        #line 82 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox patientNameTextBox2;
        
        #line default
        #line hidden
        
        
        #line 86 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox patientSurnameTextBox2;
        
        #line default
        #line hidden
        
        
        #line 90 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox doctorsList2;
        
        #line default
        #line hidden
        
        
        #line 94 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox visitStatusComboBox;
        
        #line default
        #line hidden
        
        
        #line 103 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DatePicker visitDate2;
        
        #line default
        #line hidden
        
        
        #line 106 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button findVisitButton;
        
        #line default
        #line hidden
        
        
        #line 107 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button clearFilterButton2;
        
        #line default
        #line hidden
        
        
        #line 108 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button refreshButton2;
        
        #line default
        #line hidden
        
        
        #line 114 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button cancelVisitButton;
        
        #line default
        #line hidden
        
        
        #line 115 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button cancelUndoneVisitsButton;
        
        #line default
        #line hidden
        
        
        #line 119 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ScrollViewer visitsScrollViewer;
        
        #line default
        #line hidden
        
        
        #line 121 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid visitsDataGrid;
        
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
            System.Uri resourceLocater = new System.Uri("/Rejestratorka;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\MainWindow.xaml"
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
            this.logoutMenuItem = ((System.Windows.Controls.MenuItem)(target));
            
            #line 8 "..\..\MainWindow.xaml"
            this.logoutMenuItem.Click += new System.Windows.RoutedEventHandler(this.logoutMenuItem_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.aboutMenuItem = ((System.Windows.Controls.MenuItem)(target));
            
            #line 10 "..\..\MainWindow.xaml"
            this.aboutMenuItem.Click += new System.Windows.RoutedEventHandler(this.aboutMenuItem_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.patientNameTextBox1 = ((System.Windows.Controls.TextBox)(target));
            
            #line 22 "..\..\MainWindow.xaml"
            this.patientNameTextBox1.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.PatientFilterTextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            this.patientSurnameTextBox1 = ((System.Windows.Controls.TextBox)(target));
            
            #line 26 "..\..\MainWindow.xaml"
            this.patientSurnameTextBox1.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.PatientFilterTextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.peselTextBox = ((Xceed.Wpf.Toolkit.MaskedTextBox)(target));
            
            #line 30 "..\..\MainWindow.xaml"
            this.peselTextBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.peselTextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.findPatientButton = ((System.Windows.Controls.Button)(target));
            
            #line 33 "..\..\MainWindow.xaml"
            this.findPatientButton.Click += new System.Windows.RoutedEventHandler(this.findPatientButton_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.clearFilterButton1 = ((System.Windows.Controls.Button)(target));
            
            #line 34 "..\..\MainWindow.xaml"
            this.clearFilterButton1.Click += new System.Windows.RoutedEventHandler(this.clearFilterButton1_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.addPatientButton = ((System.Windows.Controls.Button)(target));
            
            #line 35 "..\..\MainWindow.xaml"
            this.addPatientButton.Click += new System.Windows.RoutedEventHandler(this.addPatientButton_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.patientsDataGrid = ((System.Windows.Controls.DataGrid)(target));
            
            #line 42 "..\..\MainWindow.xaml"
            this.patientsDataGrid.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.patientsDataGrid_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 10:
            this.registerVisitButton = ((System.Windows.Controls.Button)(target));
            
            #line 48 "..\..\MainWindow.xaml"
            this.registerVisitButton.Click += new System.Windows.RoutedEventHandler(this.registerVisitButton_Click);
            
            #line default
            #line hidden
            return;
            case 11:
            this.TabRejScrollViewer = ((System.Windows.Controls.ScrollViewer)(target));
            return;
            case 12:
            this.doctorsList = ((System.Windows.Controls.ComboBox)(target));
            
            #line 55 "..\..\MainWindow.xaml"
            this.doctorsList.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.doctorsList_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 13:
            this.hoursOfVisitsList = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 14:
            this.visitDate = ((System.Windows.Controls.DatePicker)(target));
            
            #line 62 "..\..\MainWindow.xaml"
            this.visitDate.SelectedDateChanged += new System.EventHandler<System.Windows.Controls.SelectionChangedEventArgs>(this.visitDate_SelectedDateChanged);
            
            #line default
            #line hidden
            return;
            case 15:
            this.visitTime = ((Xceed.Wpf.Toolkit.TimePicker)(target));
            
            #line 66 "..\..\MainWindow.xaml"
            this.visitTime.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<object>(this.visitTime_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 16:
            this.registeredVisitsTab = ((System.Windows.Controls.TabItem)(target));
            return;
            case 17:
            this.patientNameTextBox2 = ((System.Windows.Controls.TextBox)(target));
            
            #line 82 "..\..\MainWindow.xaml"
            this.patientNameTextBox2.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.VisitFilterTextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 18:
            this.patientSurnameTextBox2 = ((System.Windows.Controls.TextBox)(target));
            
            #line 86 "..\..\MainWindow.xaml"
            this.patientSurnameTextBox2.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.VisitFilterTextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 19:
            this.doctorsList2 = ((System.Windows.Controls.ComboBox)(target));
            
            #line 90 "..\..\MainWindow.xaml"
            this.doctorsList2.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.doctorsList2_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 20:
            this.visitStatusComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 94 "..\..\MainWindow.xaml"
            this.visitStatusComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.visitStatusComboBox_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 21:
            this.visitDate2 = ((System.Windows.Controls.DatePicker)(target));
            
            #line 103 "..\..\MainWindow.xaml"
            this.visitDate2.SelectedDateChanged += new System.EventHandler<System.Windows.Controls.SelectionChangedEventArgs>(this.visitDate2_SelectedDateChanged);
            
            #line default
            #line hidden
            return;
            case 22:
            this.findVisitButton = ((System.Windows.Controls.Button)(target));
            
            #line 106 "..\..\MainWindow.xaml"
            this.findVisitButton.Click += new System.Windows.RoutedEventHandler(this.findVisitButton_Click);
            
            #line default
            #line hidden
            return;
            case 23:
            this.clearFilterButton2 = ((System.Windows.Controls.Button)(target));
            
            #line 107 "..\..\MainWindow.xaml"
            this.clearFilterButton2.Click += new System.Windows.RoutedEventHandler(this.clearFilterButton2_Click);
            
            #line default
            #line hidden
            return;
            case 24:
            this.refreshButton2 = ((System.Windows.Controls.Button)(target));
            
            #line 108 "..\..\MainWindow.xaml"
            this.refreshButton2.Click += new System.Windows.RoutedEventHandler(this.refreshButton_Click);
            
            #line default
            #line hidden
            return;
            case 25:
            this.cancelVisitButton = ((System.Windows.Controls.Button)(target));
            
            #line 114 "..\..\MainWindow.xaml"
            this.cancelVisitButton.Click += new System.Windows.RoutedEventHandler(this.cancelVisitButton_Click);
            
            #line default
            #line hidden
            return;
            case 26:
            this.cancelUndoneVisitsButton = ((System.Windows.Controls.Button)(target));
            
            #line 115 "..\..\MainWindow.xaml"
            this.cancelUndoneVisitsButton.Click += new System.Windows.RoutedEventHandler(this.cancelUndoneVisitsButton_Click);
            
            #line default
            #line hidden
            return;
            case 27:
            this.visitsScrollViewer = ((System.Windows.Controls.ScrollViewer)(target));
            return;
            case 28:
            this.visitsDataGrid = ((System.Windows.Controls.DataGrid)(target));
            
            #line 121 "..\..\MainWindow.xaml"
            this.visitsDataGrid.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.visitsDataGrid_SelectionChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}


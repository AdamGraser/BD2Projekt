   M S B A M L   `   `   `  ����   Administrator    Administrator.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  Administrator��$	��350��$	��525��.��]� ��. .+�o� $	��Top=�.@�m� -   $��logoutMenuItem��$��Wyloguj się��m� -   $��AddEntryItem��$
��Nowy��m� -   $��SaveChangesItem��$��Zapisz zmiany��m� $��Pomoc��.@�m� -   $��aboutMenuItem��$��O programie���� $	��Top=�.@��� -   $��RejestratorkaTabItem��$��Rejestratorka��.��&   System.Windows.Controls.DataGrid  -     Name$ RejestratorkaGrid��   AutoGenerateColumns .     SelectionMode$ Single=�   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ ID��   
IsReadOnly . 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *id_rej+  $ Nazwisko�� �� *
nazwisko+  $
 Imie�� �� *imie+  $ Login�� �� *login+  $ Haslo�� �� *haslo+4  .System.Windows.Controls.DataGridTemplateColumn  $ Wygasa��   Width5  /System.Windows.Controls.DataGridLengthConverter$	 120 	   CellTemplate	 �� .R�(  "System.Windows.Controls.DatePicker  $��0q�
   SelectedDate
 �� *wygasa+ �� UpdateSourceTrigger$ PropertyChanged=�  $ Aktywny��$	 120 	 �� .R�  {�.  
 �� *	aktywny+�� -   $��LekarzTabItem��$��Lekarz��.��  -   $ 
LekarzGrid�� .  $ Single=�   $ ID�� .  �� *id_lek+  $ Nazwisko�� �� *
nazwisko+  $
 Imie�� �� *imie+  $ Login�� �� *login+  $ Haslo�� �� *haslo+  $ Specjalizacja�� �� *
kod_spec+  $ Wygasa��$	 120 	 �� .R�  $��0q�
 �� *wygasa+$ PropertyChanged=�  $ Aktywny��$	 120 	 �� .R�  {�.  
 �� *	aktywny+�� -	   $��LaborantTabItem��$��Laborant��.��  -
   $ LaborantGrid�� .  $ Single=�   $ ID�� .  �� *id_lab+  $ Nazwisko�� �� *
nazwisko+  $
 Imie�� �� *imie+  $ Login�� �� *login+  $ Haslo�� �� *haslo+  $ 	Kierownik��	 �� .R���  d� 	IsChecked �� *kier+ �� Mode$ TwoWay=�  $ Wygasa��$	 120 	 �� .R�  $��0q�
 �� *wygasa+$ PropertyChanged=�  $ Aktywny��$	 120 	 �� .R�  {�.  
 �� *	aktywny+�� -   $��BadaniaTabItem��$��Słownik badań��.��  -   $ 
Sl_badGrid�� .  $ Single=�   $	 Kod�� .  �� *kod+  $ Nazwa�� �� *nazwa+  $
 Opis�� �� *opis+  $	 Lab��	 �� .R���  �� *lab+$ TwoWay=��� -   $��SpecTabItem��$��Słownik specjalizacji��.��  -   $ Sl_specGrid�� .  $ Single=�   $ Kod Specjalizacji�� .  �� *
kod_spec+  $ Nazwa�� �� *nazwa+
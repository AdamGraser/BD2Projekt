   M S B A M L   `   `   `  ����   Administrator*    $Administrator.AddRejestratorkaDialog   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     :xctk)http://schemas.xceed.com/wpf/xaml/toolkit       � Title$#  Dodawanie nowej rejestratorki��$	��240��$	��300�� � 
ResizeMode$ NoResize=� � WindowStyle$ 
ToolWindow=�.��]� ���	#FFE5E4E2.+�� $	��Top=�$��
10,10,10,0q� � RowDefinitions �� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto � � ColumnDefinitions �� $
��auto ��� $��* �.+��� $��Login:��$��0��$��0���� -    ��Name$ loginTextBox��$��24��$	��164��$��0��$��1�� �� TextWrapping$
 Wrap=�$��0���� $��Hasło:��$��1��$��0��G� -    G�Name$ 
pwdTextBox��$��24��$	��164��$��1��$��1��$��1���� $��Wygasa:��$��2��$��0��(  "System.Windows.Controls.DatePicker  -     Name$ DeactivateBox��$��2��$��1��$	��164����. $��2���� $��Imie:��$��3��$��0���� -   $ imieTextBox��$��24��$	��164��$��3��$��1��$
 Wrap=�$��3���� $��	Nazwisko:��$��4��$��0���� -   $ nazwiskoTextBox��$��24��$	��164��$��4��$��1��$
 Wrap=�$��4���� 	 �� Orientation$	 
Horizontal=�$��5��$��2��$��Center=�.+��� -   
 ��Name$
 okButton��$��OK��$��75�� �� 	IsDefault . {�.  $��5��$��0,0,20,0q��� -   $
 cancelButton��$��Anuluj��$��75�� �� IsCancel . $��6��
   M S B A M L   `   `   `  ����   Administrator     Administrator.AddBadDialog   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     :xctk)http://schemas.xceed.com/wpf/xaml/toolkit       � Title$*  $Dodawanie nowego badania do slownika��$	��140��$	��300�� � 
ResizeMode$ NoResize=� � WindowStyle$ 
ToolWindow=�.��� ���	#FFE5E4E2 � RowDefinitions �� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto � � ColumnDefinitions �� $
��auto ��� $��* �.+��� $��Nazwa:��$��0��$��0���� -    ��Name$ nazwaTextBox��$��24��$	��164��$��0��$��1�� �� TextWrapping$
 Wrap=�$��0���� $��Opis:��$��1��$��0���� -   $ opisTextBox��$��24��$	��164��$��1��$��1��$
 Wrap=�$��1���� $��Laboratoryjne?��$��2��$��0���� -    ��Name$ labCheckBox��$��15��$��15��$��2��$��1��$��2����  �� Orientation$ 
Horizontal=�$��3��$��2��$��Center=�.+��� -   	 ��Name$	 okButton��$��OK��$��75��
 �� 	IsDefault
 . {�.  $��3��$��0,0,20,0q��� -   $	 cancelButton��$��Anuluj��$��75�� �� IsCancel . $��4��
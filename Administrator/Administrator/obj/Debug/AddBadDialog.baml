   M S B A M L   `   `   `  ����  Administrator     Administrator.AddBadDialog   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   :xctk)http://schemas.xceed.com/wpf/xaml/toolkit     5   	     � Title$*  $Dodawanie nowego badania do slownika��5   	   $	��140��66   $	��300��6C    � 
ResizeMode$ NoResize=�6O    � WindowStyle$ 
ToolWindow=�6e   .��5      � ���	#FFE5E4E2 � RowDefinitions 5   
   �� 5      $��* �6   �� 5	      $��* �6   �� 5
      $��* �6   �� 5      $��* �6    � ColumnDefinitions 5   
   �� 5      $
��auto �6   �� 5      $��* �6   .+�5   
   �� $��Nazwa:��6   $��0��6!   $��0��6.   $��Center=�6>   �� 5   
   -   5         ��Name$ nazwaTextBox��5      $��24��6(   $	��164��64   $��0��6@   $��1��6M   $��Center=�6]   $��0��6x   6�   �� 5   
   $��Opis:��6   $��1��6    $��0��6-   $��Center=�6=   �� 5   
   -   5        $ opisTextBox��5      $��24��6'   $	��164��63   $��1��6?   $��1��6L   $��Center=�6\   $��1��6w   �� 5   
   $��Laboratoryjne?��6   $��2��6)   $��0��66   $��Center=�6F   �� 5   
   -   5         ��Name$ labCheckBox��5      $��15��6(   $��15��64   $��2��6?   $��1��6L   $��Center=�6\   $��2��6w   �� 5   
    �� Orientation$ 
Horizontal=�6   $��3��6.   $��2��6;   $��Center=�6O   $��Center=�6l   $��24��6�   .+�5      �� -   5         ��Name$ okButton��5      $��OK��6'   $��75��64   	 �� 	IsDefault	 . {�.  $��3��6y   $��0,0,20,0q�6�   �� 5      -   5        $ cancelButton��5      $��Anuluj��6+   $��75��6<   
 �� IsCancel
 . $��4��6r   5      5      5       
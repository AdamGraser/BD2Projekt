   M S B A M L   `   `   `  ����  Administrator!    Administrator.AddSpecDialog   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   :xctk)http://schemas.xceed.com/wpf/xaml/toolkit     5   	     � Title$/  )Dodawanie nowej specjalizacji do slownika��5   	   $	��100��6;   $	��300��6H    � 
ResizeMode$ NoResize=�6T    � WindowStyle$ 
ToolWindow=�6j   .��5      � ���	#FFE5E4E2 � RowDefinitions 5   
   �� 5      $��* �6   �� 5	      $��* �6    � ColumnDefinitions 5   
   �� 5      $
��auto �6   �� 5      $��* �6   .+�5   
   �� $��Nazwa:��6   $��0��6!   $��0��6.   $��Center=�6>   �� 5   
   -   5         ��Name$ nazwaTextBox��5      $��24��6(   $	��164��64   $��0��6@   $��1��6M   $��Center=�6]   $��0��6x   6�   �� 5   
    �� Orientation$ 
Horizontal=�6   $��1��6.   $��2��6;   $��Center=�6O   $��Center=�6l   $��24��6�   .+�5      �� -   5         ��Name$ okButton��5      $��OK��6'   $��75��64    �� 	IsDefault . {�.  $��1��6y   $��0,0,20,0q�6�   �� 5      -   5        $ cancelButton��5      $��Anuluj��6+   $��75��6<   	 �� IsCancel	 . $��2��6r   5      5      5      
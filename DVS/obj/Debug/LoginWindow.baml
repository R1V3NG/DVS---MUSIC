   M S B A M L   `   `   `  ����>  :DVS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:DVSDVS  5   	       DVS.LoginWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   #localclr-namespace:DVS     5   	   -   5          � Title$   Вход в профиль��5   	   $	��700��6    $	��700��6-    � WindowStyle$ ThreeDBorderWindow=�69    � WindowStartupLocation$ CenterScreen=�6Z   ���	#FF1E1E1E � 	Resources 5	       �� MergedDictionaries �� Source�� 5
   
    5      �� 5      $ DictionaryButton.xaml?�6%   �� 5      $ DictionaryBox.xaml?�6%   �� 5      $ DictionaryCheckBox.xaml?�6%   %    5      .��5      � 
������  � ShowGridLines .  .+�5   
   �� $	��600��6   $��2��5       �� Orientation$ Vertical=�5      .+�5   
   �� $��Логин:��6   $��0,98,0,0q�6!   $
��Left=�63   
�������$��40��6a   $	��100��6m   $��24(�6y   �� 5      -   5        $��tLogin��5         RoundBox#��[  6$   $��150,-35,0,0q�6F    �� TextWrapping$
 Wrap=�6[   $��Right=�6o   $	��350��6�   $��40��6�   $��26(�6�   �� 5      $��Пароль:��6   $��0,5,0,0q�6'   $
��Left=�68   
�������$��40��6f   $	��100��6r   $��24(�6~   G� 5      -   5        $��	tPassword��5        	RoundPBox#��[ 6+   $��	0,-35,0,0q�6N   $��Right=�6a   $	��Top=�6}   $	��350��6�   $��40��6�   $��26(�6�   �� 5      -   5        	 ��Name$	 RememberMeCheckBox��5        CheckBoxLarge#��[ 64   $!��Запомнить меня��6[   
�������$��20(�6�   $��
250,20,0,0q�6�   6�   �� 5      -   5        $	��Err��5      $6��0Неверный логин или пароль!��6   $��0,50,0,0q�6D   $��Center=�6V   $	��365��6s   $��24(�6   $y�Hidden=�6�   
�������$��Center=�6�   $��Center=�6�   �� 5      -   5          RoundButtonStyle#��[ 5      $��0,50,0,0q�6?   $��
Войти��6Q   $��Center=�6a   $��Center=�6~   
�������$	��100��6�   $	��300��6�   $��24(�6�   �� 5      -   5        #��[ 5      $#��Создать аккаунт��6?   $��0,50,0,0q�6Y   
�������$��Center=�6~   $��Center=�6�   $	��100��6�   $	��300��6�   $��24(�6�   5       5!      5"      
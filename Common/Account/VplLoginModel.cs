using System.ComponentModel.DataAnnotations;

namespace Common.Account;

[Serializable]
public class VplLoginModel
{
    [Required(ErrorMessage = "Вы забыли заполнить поле \"Код\"")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Код не должен быть больше 20 и меньше 3 симоволов")]
    public string Code { get; set; }
    
    
    
    [Required(ErrorMessage = "Для входа необходимо знать пароль")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Пароль не должен быть больше 20 и меньше 5 симоволов")]
    public string Password { get; set; }
}
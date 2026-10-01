using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "名前は必須です")]
        [MaxLength(10, ErrorMessage = "名前は10文字以内で入力してください")]
        public string Name { get; set; } = string.Empty;

        [Range(0, 10000, ErrorMessage = "価格は0以上10000以下で入力してください")]
        public decimal Price { get; set; }

        public Product() { }

        // コピーコンストラクタ
        public Product(Product other)
        {
            ArgumentNullException.ThrowIfNull(other);

            Id = other.Id;
            Name = other.Name;
            Price = other.Price;
        }
    }
}

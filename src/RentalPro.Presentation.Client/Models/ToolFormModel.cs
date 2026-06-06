using System.ComponentModel.DataAnnotations;

namespace RentalPro.Presentation.Client.Models;

public sealed class ToolFormModel
{
    [Required(ErrorMessage = "Укажите артикул")]
    public string ArticleNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите название")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Выберите категорию")]
    public Guid? CategoryId { get; set; }

    [Required(ErrorMessage = "Выберите производителя")]
    public Guid? ManufacturerId { get; set; }

    [Required(ErrorMessage = "Выберите статус")]
    public Guid? StatusId { get; set; }

    [Range(1, 1_000_000, ErrorMessage = "Стоимость аренды должна быть больше 0")]
    public decimal RentalPricePerDay { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Залог не может быть отрицательным")]
    public decimal DepositAmount { get; set; }

    [Required(ErrorMessage = "Укажите серийный номер")]
    public string SerialNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите инвентарный номер")]
    public string InventoryNumber { get; set; } = string.Empty;

    public string? CurrentCondition { get; set; }

    public string? PhotoPath { get; set; }
}
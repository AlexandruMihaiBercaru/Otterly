using System.ComponentModel.DataAnnotations;

namespace Proj.Models;

public static class LabelCommand
{
    public record Create(
        [StringLength(32)]
        [Required (ErrorMessage = "Numele etichetei este necesar.")]
        string Name,

        [Required (ErrorMessage = "Trebuie sa adaugati o culoare.")]
        [StringLength(maximumLength: 7)]
        [RegularExpression("#[A-Fa-f0-9]{6}")]
        string Color
    );
    
}
public class Label
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public Guid ProjectId { get; init; }

    public string Name { get; set; }
    public string Color { get; set; }

    public Label() { }
    private Label(string _name, string _color, Guid _projectId)
    {
        Name = _name;
        Color = _color;
        ProjectId = _projectId;
    }

    public static Label From(LabelCommand.Create cmd, Guid _projectId)
    {
        var label = new Label(cmd.Name, cmd.Color, _projectId);
        return label;
    }


}

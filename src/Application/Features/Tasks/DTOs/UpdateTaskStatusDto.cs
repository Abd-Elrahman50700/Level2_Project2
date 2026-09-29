using Domain.Enums;

namespace Application.Features.Tasks.DTOs;

public class UpdateTaskStatusDto
{
    public TaskStatus Status { get; set; }
}

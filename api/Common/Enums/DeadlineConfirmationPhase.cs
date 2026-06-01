namespace Common.Enums;

/// <summary>Этап запроса подтверждения готовности к сроку доставки (за 2 дня).</summary>
public enum DeadlineConfirmationPhase
{
    None = 0,
    FirstRequest = 1,
    Reminder = 2
}

using System.Collections.ObjectModel;
using StayHub.Contracts.Assistant;
using StayHub.Mobile.Models;
using StayHub.Mobile.Services;

namespace StayHub.Mobile.Views;

public partial class AssistantPage : ContentPage
{
    private const double TabBarClearance = 104;
    private const double KeyboardClearance = 12;
    private readonly IAssistantService _assistantService;
    private bool _isSending;

    public AssistantPage(IAssistantService assistantService)
    {
        InitializeComponent();
        _assistantService = assistantService;
        BindingContext = this;
    }

    public ObservableCollection<AssistantMessageViewModel> Messages { get; } = [];

    private async void OnSendClicked(object? sender, EventArgs e) => await SendCurrentQuestionAsync();

    private void OnQuestionEntryFocused(object? sender, FocusEventArgs e) =>
        SetBottomClearance(KeyboardClearance);

    private void OnQuestionEntryUnfocused(object? sender, FocusEventArgs e) =>
        SetBottomClearance(TabBarClearance);

    private async void OnNextArrivalSuggestionClicked(object? sender, EventArgs e)
    {
        QuestionEntry.Text = "Who is arriving next?";
        await SendCurrentQuestionAsync();
    }

    private async void OnTodayDeparturesSuggestionClicked(object? sender, EventArgs e)
    {
        QuestionEntry.Text = "Who is checking out today?";
        await SendCurrentQuestionAsync();
    }

    private async Task SendCurrentQuestionAsync()
    {
        var question = QuestionEntry.Text?.Trim();
        if (_isSending || string.IsNullOrWhiteSpace(question))
            return;

        var history = Messages
            .Select(message => new AssistantConversationMessage(message.Role, message.Content))
            .ToList();

        Messages.Add(new AssistantMessageViewModel("user", question));
        EmptyState.IsVisible = false;
        QuestionEntry.Text = string.Empty;
        SetSendingState(true);

        try
        {
            var response = await _assistantService.SendAsync(new AssistantChatRequest
            {
                Message = question,
                History = history
            });
            Messages.Add(new AssistantMessageViewModel("assistant", response.Answer));
        }
        catch (UnauthorizedAccessException)
        {
            Messages.Add(new AssistantMessageViewModel(
                "assistant",
                "Your session expired. Please sign in again."));
        }
        catch (ApiException exception)
        {
            Messages.Add(new AssistantMessageViewModel("assistant", exception.Message));
        }
        finally
        {
            SetSendingState(false);
            if (Messages.Count > 0)
                MessagesView.ScrollTo(Messages[^1], position: ScrollToPosition.End, animate: true);
        }
    }

    private void SetSendingState(bool isSending)
    {
        _isSending = isSending;
        QuestionEntry.IsEnabled = !isSending;
        SendButton.IsVisible = !isSending;
        SendingIndicator.IsVisible = isSending;
        SendingIndicator.IsRunning = isSending;
    }

    private void SetBottomClearance(double bottom) =>
        PageLayout.Padding = new Thickness(20, 12, 20, bottom);
}

# Material Components Usage Guide for GolfManager

## 🎨 Quick Reference

This guide shows how to use FifthBox Material Components in the GolfManager project with practical examples specific to golf league management.

## 📋 Common Patterns

### League Dashboard Card

```razor
<MaterialContainer Size="MaterialContainerSize.Large">
    <MaterialCard Variant="MaterialCardVariant.Filled">
        <div class="md-card__content p-6">
            <h2 class="md-card__title">@League.Name</h2>
            <p class="md-card__subtitle">Season @Season.Year</p>
            
            <div class="mt-4 space-y-2">
                <p class="text-sm">Active Players: @PlayerCount</p>
                <p class="text-sm">Upcoming Events: @EventCount</p>
            </div>
            
            <div class="mt-6 flex gap-2">
                <MaterialButton Style="MaterialButtonStyle.Filled"
                               Text="View Leaderboard"
                               OnClick="@(() => NavigateToLeaderboard())" />
                <MaterialButton Style="MaterialButtonStyle.Outlined"
                               Text="Manage League"
                               OnClick="@(() => NavigateToSettings())" />
            </div>
        </div>
    </MaterialCard>
</MaterialContainer>
```

### Score Entry Form

```razor
<MaterialCard Variant="MaterialCardVariant.Outlined">
    <div class="md-card__content p-6">
        <h3 class="md-card__title">Enter Score</h3>
        
        <div class="space-y-4 mt-4">
            <MaterialSelect @bind-Value="selectedGolfer"
                           Label="Golfer"
                           Items="golfers"
                           ValueSelector="@(g => g.Id)"
                           DisplaySelector="@(g => g.Name)" />
            
            <MaterialInput @bind-Value="score"
                          Label="Score"
                          Type="number"
                          Variant="MaterialInputVariant.Outlined"
                          Min="0"
                          Max="200" />
            
            <MaterialInput @bind-Value="putts"
                          Label="Putts"
                          Type="number"
                          Variant="MaterialInputVariant.Outlined" />
            
            <MaterialCheckbox @bind-Value="fairwayHit"
                             Label="Fairway Hit" />
            
            <MaterialCheckbox @bind-Value="greenInRegulation"
                             Label="Green in Regulation" />
        </div>
        
        <div class="mt-6 flex gap-2 justify-end">
            <MaterialButton Style="MaterialButtonStyle.Text"
                           Text="Cancel"
                           OnClick="HandleCancel" />
            <MaterialButton Style="MaterialButtonStyle.Filled"
                           Text="Save Score"
                           OnClick="HandleSaveScore" />
        </div>
    </div>
</MaterialCard>
```

### Player Selection with Chips

```razor
<div class="space-y-2">
    <label class="text-sm font-medium">Select Team Members</label>
    <div class="flex flex-wrap gap-2">
        @foreach (var player in availablePlayers)
        {
            <MaterialChip Variant="@(selectedPlayers.Contains(player) ? MaterialChipVariant.Filter : MaterialChipVariant.Assist)"
                         Text="@player.Name"
                         Selected="@selectedPlayers.Contains(player)"
                         OnClick="@(() => TogglePlayer(player))" />
        }
    </div>
</div>
```

### Event Status Tabs

```razor
<MaterialTabs @bind-ActiveTab="activeTab">
    <MaterialTab Label="Upcoming" Value="upcoming">
        <UpcomingEventsComponent />
    </MaterialTab>
    <MaterialTab Label="In Progress" Value="inprogress">
        <InProgressEventsComponent />
    </MaterialTab>
    <MaterialTab Label="Completed" Value="completed">
        <CompletedEventsComponent />
    </MaterialTab>
</MaterialTabs>
```

### Confirmation Dialog

```razor
<MaterialDialog @bind-IsOpen="showDeleteDialog"
                Title="Delete League?"
                MaxWidth="400px">
    <p class="mb-4">Are you sure you want to delete this league? This action cannot be undone.</p>
    
    <div class="flex gap-2 justify-end">
        <MaterialButton Style="MaterialButtonStyle.Text"
                       Text="Cancel"
                       OnClick="@(() => showDeleteDialog = false)" />
        <MaterialButton Style="MaterialButtonStyle.Filled"
                       Text="Delete"
                       OnClick="HandleDeleteLeague" />
    </div>
</MaterialDialog>

@code {
    private bool showDeleteDialog = false;
    
    private void ShowDeleteConfirmation()
    {
        showDeleteDialog = true;
    }
    
    private async Task HandleDeleteLeague()
    {
        await LeagueService.DeleteLeagueAsync(leagueId);
        showDeleteDialog = false;
        NavigationManager.NavigateTo("/leagues");
    }
}
```

### Success/Error Notifications

```razor
<MaterialSnackbar @bind-IsOpen="showSuccessMessage"
                  Message="Score saved successfully!"
                  Position="MaterialSnackbarPosition.TopCenter"
                  Duration="3000" />

<MaterialSnackbar @bind-IsOpen="showErrorMessage"
                  Message="@errorMessage"
                  Position="MaterialSnackbarPosition.TopCenter"
                  Duration="5000" />

@code {
    private bool showSuccessMessage = false;
    private bool showErrorMessage = false;
    private string errorMessage = "";
    
    private async Task HandleSaveScore()
    {
        try
        {
            await ScoreService.SaveScoreAsync(score);
            showSuccessMessage = true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            showErrorMessage = true;
        }
    }
}
```

### Loading State

```razor
@if (isLoading)
{
    <div class="flex items-center justify-center p-8">
        <MaterialProgressIndicator Type="MaterialProgressType.Circular"
                                   Size="MaterialProgressSize.Large" />
    </div>
}
else
{
    <LeaderboardTable Data="@leaderboardData" />
}
```

### Responsive Data Table

```razor
<MaterialTable Items="@golfers"
               TItem="GolferDto"
               Striped="true"
               Hoverable="true">
    <MaterialTableColumn Property="@(g => g.Name)" Title="Name" Sortable="true" />
    <MaterialTableColumn Property="@(g => g.Handicap)" Title="Handicap" Sortable="true" />
    <MaterialTableColumn Property="@(g => g.AverageScore)" Title="Avg Score" Sortable="true" />
    <MaterialTableColumn Property="@(g => g.RoundsPlayed)" Title="Rounds" Sortable="true" />
    <MaterialTableColumn Title="Actions">
        <Template Context="golfer">
            <MaterialIconButton Icon="MaterialIconType.Edit"
                               Style="MaterialIconButtonStyle.Standard"
                               OnClick="@(() => EditGolfer(golfer))" />
            <MaterialIconButton Icon="MaterialIconType.Delete"
                               Style="MaterialIconButtonStyle.Standard"
                               OnClick="@(() => DeleteGolfer(golfer))" />
        </Template>
    </MaterialTableColumn>
</MaterialTable>
```

### Floating Action Button for Quick Actions

```razor
<MaterialFab Icon="MaterialIconType.Add"
             Position="MaterialFabPosition.BottomRight"
             Size="MaterialFabSize.Large"
             OnClick="@(() => NavigateTo("/scores/new"))"
             AriaLabel="Add new score" />
```

### Settings Panel (Offcanvas)

```razor
<MaterialIconButton Icon="MaterialIconType.Settings"
                   OnClick="@(() => showSettings = true)" />

<MaterialOffcanvas @bind-IsOpen="showSettings"
                   Position="MaterialOffcanvasPosition.Right"
                   Title="League Settings">
    <div class="space-y-4 p-4">
        <MaterialSwitch @bind-Value="enableHandicaps"
                       Label="Enable Handicaps" />
        
        <MaterialSwitch @bind-Value="allowGuestPlayers"
                       Label="Allow Guest Players" />
        
        <MaterialSelect @bind-Value="scoringType"
                       Label="Scoring Type"
                       Items="scoringTypes" />
        
        <MaterialButton Style="MaterialButtonStyle.Filled"
                       Text="Save Settings"
                       OnClick="HandleSaveSettings"
                       Class="w-full" />
    </div>
</MaterialOffcanvas>
```

## 🎨 Combining with Tailwind CSS

Material Components work great with Tailwind utilities:

```razor
<MaterialCard Variant="MaterialCardVariant.Filled" Class="hover:shadow-lg transition-shadow">
    <div class="md-card__content p-6">
        <div class="flex items-center justify-between mb-4">
            <h3 class="text-xl font-bold">@Event.Name</h3>
            <MaterialChip Variant="MaterialChipVariant.Assist"
                         Text="@Event.Status"
                         Class="bg-green-100 text-green-800" />
        </div>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
                <p class="text-sm text-gray-600">Date</p>
                <p class="font-medium">@Event.Date.ToShortDateString()</p>
            </div>
            <div>
                <p class="text-sm text-gray-600">Course</p>
                <p class="font-medium">@Event.CourseName</p>
            </div>
        </div>
    </div>
</MaterialCard>
```

## 📚 More Examples

For more component examples, see:
- [Material Components Demo](../fifthbox-materialcomponents/demo)
- [Component Documentation](../fifthbox-materialcomponents/README.md)


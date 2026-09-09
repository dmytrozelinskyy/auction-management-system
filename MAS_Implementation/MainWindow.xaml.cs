using System.Windows;
using System.Windows.Controls;
using MAS_Implementation.Enums;
using MAS_Implementation.Models;
using MAS_Implementation.Services;

namespace MAS_Implementation;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly AmsService _service = new();
    private Cataloguer? _currentCataloguer;

    public MainWindow()
    {
        InitializeComponent();
        LoadCurrentCataloguer();
        LoadInitialData();
    }

    private void LoadCurrentCataloguer()
    {
        _currentCataloguer = _service.GetCataloguer();
    }

    private void LoadInitialData()
    {
        ConsignmentAgreementsBox.ItemsSource = _service.GetActiveAgreements();
        AuctionsList.ItemsSource = _service.GetAuctionsWithPaddlesAndBidders();
        LotsList.ItemsSource = _service.GetLots();
        
        CategoriesListBox.ItemsSource = AmsConfig.Categories;
    }

    private void AuctionListTab_Click(object sender, RoutedEventArgs e)
    {
        MainTabs.SelectedIndex = 0;
    }
    
    private void HistoryTab_Click(object sender, RoutedEventArgs e)
    {
        MainTabs.SelectedIndex = 1;
    }
    
    private void CatalogTab_Click(object sender, RoutedEventArgs e)
    {
        MainTabs.SelectedIndex = 2;
    }
    
    private void CatalogLotTab_Click(object sender, RoutedEventArgs e)
    {
        MainTabs.SelectedIndex = 3;
    }
    
    private void AnalyticsTab_Click(object sender, RoutedEventArgs e)
    {
        MainTabs.SelectedIndex = 4;
    }
    
    private void ConsignmentAgreementsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ConsignmentAgreementsBox.SelectedItem is ConsignmentAgreement agreement)
            SelectedAgreementText.Text = $"Selected: {agreement}";
        else SelectedAgreementText.Text = $"Selected: ...";
    }

    private void CategoriesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategoriesListBox.SelectedItem is string category)
            SelectedCategoryText.Text = $"Selected: {category}";
        else
            SelectedCategoryText.Text = "Selected: ...";
    }

    private void AuctionsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AuctionsList.SelectedItem is not Auction selected) return;
        
        PaddlesList.ItemsSource = selected.Paddles;
    }
    
    private void LotsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LotsList.SelectedItem is not Lot selectedLot) return;
        
        TitleText.Text = selectedLot.Title;
        DescriptionText.Text = selectedLot.Description ?? "N/A";
        ConditionText.Text = selectedLot.Condition;
        EstimateLowText.Text = selectedLot.EstimateLow.ToString();
        EstimateHighText.Text = selectedLot.EstimateHigh.ToString();
        ReservePriceText.Text = selectedLot.ReservePrice.ToString();
        CountryOfOriginText.Text = selectedLot.CountryOfOrigin;
    }
    
    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        if (ConsignmentAgreementsBox.SelectedItem is not ConsignmentAgreement selectedAgreement)
        {
            ShowNotification("Please select a consignment agreement.", isError: true);
            return;
        }

        if (CategoriesListBox.SelectedItem is not string selectedCategory)
        {
            ShowNotification("Please select a category.", isError: true);
            return;
        }

        if (!ValidateForm(out string validationError))
        {
            ShowNotification(validationError, isError: true);
            return;
        }

        var title = TitleBox.Text.Trim();
        var description = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? null : DescriptionBox.Text.Trim();
        var condition = ConditionBox.Text.Trim();
        var estimateLow = double.Parse(EstimateLowBox.Text);
        var estimateHigh = double.Parse(EstimateHighBox.Text);
        var reservePrice = double.Parse(ReservePriceBox.Text);
        var country = CountryOfOriginBox.Text.Trim();

        try
        {
            var lot = _currentCataloguer!.CatalogLot(
                title, condition,
                estimateLow, estimateHigh,
                reservePrice, country, selectedCategory, selectedAgreement, description);

            if (lot.Status == LotStatus.Blocked)
            {
                _service.SaveLot(lot);
                ShowNotification(
                    $"Lot was blocked from sales.\nCountry of Origin '{country}' is on the sanctioned/restricted list.",
                    isError: true);
                ClearForm();
                return;
            }

            if (lot.VerificationTasks.Count > 0)
            {
                var submissionPopup = new SubmissionPopup(lot, _service);
                submissionPopup.Owner = this;
                var result = submissionPopup.ShowDialog();

                if (result != true)
                    return;
            }

            _service.SaveLot(lot);

            ShowNotification(
                lot.Status == LotStatus.Eligible
                    ? $"Lot '{title}' catalogued successfully and is Eligble for auction."
                    : $"Lot '{title}' saved with status {lot.Status}. Verifications pending...",
                isError: false);

            ClearForm();
            LoadInitialData();
        }
        catch (InvalidOperationException ex)
        {
            ShowNotification(ex.Message, isError: true);
        }
        catch (ArgumentException ex)
        {
            ShowNotification(ex.Message, isError: true);
        }
    }
    
    private bool ValidateForm(out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(TitleBox.Text))
        { error = "Lot details are invalid\nTitle cannot be empty."; return false; }
        
        if (string.IsNullOrWhiteSpace(ConditionBox.Text))
        { error = "Lot details are invalid\nCondition cannot be empty."; return false; }
        
        if (!double.TryParse(EstimateLowBox.Text, out var low) || low <= 0)
        { error = "Lot details are invalid\nEstimate low must be a positive number."; return false; }

        if (!double.TryParse(EstimateHighBox.Text, out var high) || high <= 0)
        { error = "Lot details are invalid\nEstimate high must be a positive number."; return false; }

        if (high < low)
        { error = "Lot details are invalid\nEstimate high < estimate low."; return false; }
        
        if (!double.TryParse(ReservePriceBox.Text, out var reserve) || reserve <= 0)
        { error = "Lot details are invalid\nReserve price must be a positive number."; return false; }
        
        if (string.IsNullOrWhiteSpace(CountryOfOriginBox.Text))
        { error = "Lot details are invalid\nCountry of origin cannot be empty."; return false; }

        return true;
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e) => ClearForm();
    
    private void ClearForm()
    {
        TitleBox.Text = DescriptionBox.Text = ConditionBox.Text = string.Empty;
        EstimateLowBox.Text = EstimateHighBox.Text = ReservePriceBox.Text = string.Empty;
        CountryOfOriginBox.Text = string.Empty;
        ConsignmentAgreementsBox.SelectedItem = null;
        CategoriesListBox.SelectedItem = null;
        SelectedAgreementText.Text = SelectedCategoryText.Text = string.Empty;
    }
    private void ShowNotification(string msg, bool isError)
    {
        var popup = new NotificationPopup(msg, isError) {Owner = this};
        popup.ShowDialog();
    }
    
    protected override void OnClosed(EventArgs e)
    {
        _service.Dispose();
        base.OnClosed(e);
    }
}
using System.DirectoryServices;
using System.IO;
using System.Windows;
using MAS_Implementation.Enums;
using MAS_Implementation.Models;
using MAS_Implementation.Services;
using Microsoft.Win32;

namespace MAS_Implementation;

public partial class SubmissionPopup : Window
{
    private readonly Lot _lot;
    private readonly AmsService _service;

    private string? _regulatedFilePath;
    private string? _appraisalFilePath;
    private string? _insuranceFilePath;
    private string? _provenanceFilePath;
    
    public SubmissionPopup(Lot lot, AmsService service)
    {
        InitializeComponent();
        _lot = lot;
        _service = service;
        ConfigurePanels();
    }

    private void ConfigurePanels()
    {
        var count = _lot.VerificationTasks.Count;
        SubtitleText.Text = $"There {(count == 1 ? "is" : "are")} " +
                            $"{count} restricted flag{(count == 1 ? "" : "s")} on this lot:";
        var types = _lot.VerificationTasks.Select(t => t.Type).ToHashSet();
        ConfigureColumn(types.Contains(RestrictionType.Regulated), RegulatedForm, RegulatedFine);
        ConfigureColumn(types.Contains(RestrictionType.HighValue), HighValueForm, HighValueFine);
        ConfigureColumn(types.Contains(RestrictionType.Imported), ImportedForm, ImportedFine);
    }

    private static void ConfigureColumn(bool isRestricted, UIElement form, UIElement fine)
    {
        form.Visibility = isRestricted ? Visibility.Visible : Visibility.Collapsed;
        fine.Visibility = isRestricted ? Visibility.Collapsed : Visibility.Visible;
    }

    private void RegulatedFileButton_Click(object sender, RoutedEventArgs e)
    {
        _regulatedFilePath = PickFile();
        RegulatedFileName.Text = _regulatedFilePath != null ? 
            Path.GetFileName(_regulatedFilePath) :
            "No file chosen";
    }
    
    private void AppraisalFileButton_Click(object sender, RoutedEventArgs e)
    {
        _appraisalFilePath = PickFile();
        AppraisalFileName.Text = _appraisalFilePath != null ? 
            Path.GetFileName(_appraisalFilePath) :
            "No file chosen";
    }
    
    private void InsuranceFileButton_Click(object sender, RoutedEventArgs e)
    {
        _insuranceFilePath = PickFile();
        InsuranceFileName.Text = _insuranceFilePath != null ? 
            Path.GetFileName(_insuranceFilePath) :
            "No file chosen";
    }
    
    private void ProvenanceFileButton_Click(object sender, RoutedEventArgs e)
    {
        _provenanceFilePath = PickFile();
        ProvenanceFileName.Text = _provenanceFilePath != null ? 
            Path.GetFileName(_provenanceFilePath) :
            "No file chosen";
    }

    private static string? PickFile()
    {
        var dialog = new OpenFileDialog()
        {
            Filter = "Documents (*.pdf;*.docx;*.jpg|*.pdf;*.docx;*.jpg|All files (*.*)|*.*"
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private void SubmitVerificationsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            foreach (var task in _lot.VerificationTasks)
            {
                switch (task.Type)
                {
                    case RestrictionType.Regulated:
                        ProcessRegulated(task);
                        break;
                    case RestrictionType.HighValue:
                        ProcessHighValue(task);
                        break;
                    case RestrictionType.Imported:
                        ProcessImported(task);
                        break;
                }
            }

            _lot.DetermineStatus(
                _lot.AreAllVerificationsComplete() ? LotStatus.Eligible : LotStatus.PendingVerification);

            DialogResult = true;
            Close();
        }
        catch (InvalidOperationException ex)
        {
            ShowError(ex.Message);
        }
    }

    private void ProcessRegulated(VerificationTask task)
    {
        if (RegulatedPendingCheck.IsChecked == true)
        {
            task.MarkPending();
            return;
        }

        if (_regulatedFilePath != null)
        {
            task.AddDocument(new Document(
                Path.GetFileName(_regulatedFilePath),
                _regulatedFilePath,
                DocumentType.RegulatoryPermit,
                RegulatedAuthorityBox.Text,
                RegulatedValidityDate.SelectedDate ?? DateTime.Today));
        }

        task.CustomCleranceNumber = null;
        task.MarkComplete();
    }
    
    private void ProcessHighValue(VerificationTask task)
    {
        if (HighValuePendingCheck.IsChecked == true)
        {
            task.MarkPending();
            return;
        }

        if (_appraisalFilePath != null)
            task.AddDocument(new Document(
                Path.GetFileName(_appraisalFilePath),
                _appraisalFilePath,
                DocumentType.ThirdPartyAppraisal));

        if (_insuranceFilePath != null)
            task.AddDocument(new Document(
                Path.GetFileName(_insuranceFilePath),
                _insuranceFilePath,
                DocumentType.InsuranceCertificate));

        task.MarkComplete();
    }
    
    private void ProcessImported(VerificationTask task)
    {
        if (ImportedPendingCheck.IsChecked == true)
        {
            task.MarkPending();
            return;
        }

        if (_provenanceFilePath != null)
            task.AddDocument(new Document(
                Path.GetFileName(_provenanceFilePath),
                _provenanceFilePath,
                DocumentType.ProvenanceChainDocument));

        task.CustomCleranceNumber = CidnBox.Text.Trim();
        task.MarkComplete();
    }

    private void ShowError(string msg)
    {
        var popup = new NotificationPopup(
            $"Uploaded documents are invalid\n{msg}", isError: true)
            {Owner = this};
        popup.ShowDialog();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
# ScrapeWebForms
In Georgia a candidate must register with GA Gov Transparency and Campaign Finance Commission (we call it the Ethics site) before they can begin soliciting funds.  These registrations serve as an early warning to interested observers.  The related project here, ScrapeGaSOS scrape is of the Secretary of State site of the list of registered campaigns.

The program is a Microsoft Windows Forms programs written in C#.  The development solutions are divided into two projects: a Windows UI and a worker library.  

The Ethics scrape was the more difficult of the two since their site was built using Microsoft Web Forms and has a paged format.  After selecting search parameters a POST gets a listing with “Page 1 of X” at the bottom of the first page.  The first page data is formatted into CSV data and the next page is requested via a POST.  These Web Forms maintain state using the dreaded “view state” data that is returned to the server in each post so that has to be included in the scraper POST.  Once all pages are done the UI offers options to save the data to a CSV file.


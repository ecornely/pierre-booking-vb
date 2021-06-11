Imports System
Imports System.IO
Imports System.Reflection
Imports System.Collections.ObjectModel
Imports System.Linq
Imports OpenQA.Selenium

Imports OpenQA.Selenium.Chrome
Imports OpenQA.Selenium.Interactions
Imports OpenQA.Selenium.Keys
Imports OpenQA.Selenium.Interactions.Actions

Namespace pierre_booking_dotnet
    Module Program
        Private searchString as String = "Vendée"
        Function Main(ByVal args As String()) As Integer
            Dim nextSaturday as DateTime = calculateNextSatruday()
            Dim prices As List(Of String) = new List(Of String)

            Using driver = New ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                driver.Navigate().GoToUrl("https://www.booking.com")
                driver.findElement(By.id("ss")).sendKeys(searchString)
                Dim datePickers As ReadOnlyCollection(Of IWebElement) = driver.findElements(By.cssSelector("span.sb-date-field__icon.sb-date-field__icon-btn.bk-svg-wrapper.calendar-restructure-sb"))
                datePickers(0).click

                Dim allDates As ReadOnlyCollection(Of IWebElement) = driver.findElements(By.cssSelector("td.bui-calendar__date"))

                Dim startDate as IWebElement = allDates.Where(Function(e As IWebElement) e.getAttribute("data-date") IsNot Nothing AndAlso nextSaturday.ToString("yyyy-MM-dd").equals(e.getAttribute("data-date"))).Single
                startDate.findElement(By.cssSelector("span span")).click
                Dim endDate as IWebElement = allDates.Where(Function(e As IWebElement) e.getAttribute("data-date") IsNot Nothing AndAlso nextSaturday.AddDays(1).ToString("yyyy-MM-dd").equals(e.getAttribute("data-date"))).Single
                endDate.findElement(By.cssSelector("span span")).click

                driver.findElement(By.cssSelector("div.sb-searchbox-submit-col.-submit-button > button")).click

                System.Threading.Thread.Sleep(2000)

                Dim pageNumber As Integer = 1
                Dim lastPage As Integer = Integer.Parse(driver.findElement(By.cssSelector("ul.bui-pagination__list li.bui-pagination__item.sr_pagination_item:last-child div.bui-u-inline")).Text)

                Dim js As IJavaScriptExecutor = TryCast(driver, IJavaScriptExecutor)

                While pageNumber <= lastPage
                    Dim pricesElements As ReadOnlyCollection(Of IWebElement) = driver.findElements(By.cssSelector("div.bui-price-display__value.prco-inline-block-maker-helper"))
                    For Each e in pricesElements
                        prices.Add(e.Text)
                    Next
                    Dim nextPageString As Integer = pageNumber+1

                    Dim pageItems As ReadOnlyCollection(Of IWebElement) = driver.findElements(By.cssSelector("ul.bui-pagination__list li.bui-pagination__item.sr_pagination_item"))
                    Dim nextPageItems As List(Of IWebElement) = pageItems.Where(Function(e) e.findElement(By.cssSelector("div.bui-u-inline")).Text.equals(CStr(nextPageString))).ToList
                    Dim nextPageLinks As List(Of IWebElement) = nextPageItems.Select(Function(e) e.findElement(By.tagName("a"))).ToList
                    If nextPageLinks.Count = 1 Then
                        js.executeScript("window.scrollTo(0, document.body.scrollHeight)")
                        nextPageLinks(0).click
                        System.Threading.Thread.Sleep(2000)
                    End If
                    pageNumber+=1
                End While
            End Using

            Console.WriteLine("All prices are :" & String.Join(",", prices.ToArray))
            Return 0
        End Function

        Function calculateNextSatruday() As DateTime
            Dim saturday As DateTime = DateTime.Now

            Select Case DateTime.Now.DayOfWeek
                Case System.DayOfWeek.Sunday
                    saturday = DateTime.Now.AddDays(6)
                Case System.DayOfWeek.Monday
                    saturday = DateTime.Now.AddDays(5)
                Case System.DayOfWeek.Tuesday
                    saturday = DateTime.Now.AddDays(4)
                Case System.DayOfWeek.Wednesday
                    saturday = DateTime.Now.AddDays(3)
                Case System.DayOfWeek.Thursday
                    saturday = DateTime.Now.AddDays(2)
                Case System.DayOfWeek.Friday
                    saturday = DateTime.Now.AddDays(1)
            End Select

            Return saturday
        End Function
    End Module
End Namespace

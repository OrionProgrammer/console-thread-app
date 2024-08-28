using System.Collections.Concurrent;
using System.Text.Json;
using System.Xml.Serialization;

ConcurrentBag<int> globalList = new ConcurrentBag<int>();
object lockObj = new object();
int itemCount = 0;
int maxItemCount = 1000000;
int thirdThreadStartCount = 250000;
ManualResetEvent thirdThreadEvent = new ManualResetEvent(false);

Thread oddThread = new Thread(GenerateOddNumbers);
Thread primeThread = new Thread(GenerateNegativePrimes);
oddThread.Start();
primeThread.Start();

thirdThreadEvent.WaitOne(); // Wait until the global list reaches 250,000 items

Thread evenThread = new Thread(GenerateEvenNumbers);
evenThread.Start();

oddThread.Join();
primeThread.Join();
evenThread.Join();

ProcessFinalList();


void GenerateOddNumbers()
{
    Random random = new Random();
    while (itemCount < maxItemCount)
    {
        int number = random.Next(1, int.MaxValue);
        if (number % 2 != 0)
        {
            AddToGlobalList(number);
        }
    }
}

void GenerateNegativePrimes()
{
    int number = 2;
    while (itemCount < maxItemCount)
    {
        if (IsPrime(number))
        {
            AddToGlobalList(-number);
        }
        number++;
    }
}

void GenerateEvenNumbers()
{
    Random random = new Random();
    while (itemCount < maxItemCount)
    {
        int number = random.Next(1, int.MaxValue);
        if (number % 2 == 0)
        {
            AddToGlobalList(number);
        }
    }
}

bool IsPrime(int number)
{
    if (number < 2) return false;
    for (int i = 2; i <= Math.Sqrt(number); i++)
    {
        if (number % i == 0) return false;
    }
    return true;
}

void AddToGlobalList(int number)
{
    lock (lockObj)
    {
        if (itemCount < maxItemCount)
        {
            globalList.Add(number);
            itemCount++;

            if (itemCount == thirdThreadStartCount)
            {
                thirdThreadEvent.Set(); // Signal to start the third thread
            }
        }
    }
}

void ProcessFinalList()
{
    var sortedList = globalList.OrderBy(x => x).ToList();
    int oddCount = sortedList.Count(x => x % 2 != 0);
    int evenCount = sortedList.Count - oddCount;

    Console.WriteLine($"Odd numbers: {oddCount}");
    Console.WriteLine($"Even numbers: {evenCount}");

    SerializeToBinary(sortedList);
    SerializeToXml(sortedList);
}

void SerializeToBinary(List<int> list)
{
    using (FileStream fs = new FileStream("C:\\GlobalList.bin", FileMode.Create))
    {
        string jsonString = JsonSerializer.Serialize(list);
        byte[] jsonData = System.Text.Encoding.UTF8.GetBytes(jsonString);
        fs.Write(jsonData, 0, jsonData.Length);
    }
}

void SerializeToXml(List<int> list)
{
    using (FileStream fs = new FileStream("C:\\GlobalList.xml", FileMode.Create))
    {
        XmlSerializer serializer = new XmlSerializer(typeof(List<int>));
        serializer.Serialize(fs, list);
    }
}
using System.Text;
using System;

public static class CurrencyFormatter
{
    // 단위 배열: 만, 억, 조, 경, 해, 자
    private static readonly string[] Units = { "", "만", "억", "조", "경", "해", "자" };

    public static string FormatKorean(double value)
    {
        // 🌟 혹시 모를 소수점 아래 실제 골드는 먼저 버림
        double tempValue = Math.Floor(value);

        if (tempValue < 10000) return tempValue.ToString();

        int unitIndex = 0;
        double lowValueDouble = 0;
        
        // 🌟 핵심 로직: 10000 단위로 자르면서 '나머지'를 안전하게 추출
        while (tempValue >= 10000 && unitIndex < Units.Length - 1)
        {
            // 현재 단위에서의 하위 숫자 4자리를 정확하게 저장 (예: 999999999 % 10000 = 9999)
            lowValueDouble = tempValue % 10000; 
            
            // 상위 숫자로 갱신 (예: 999999999 / 10000 = 99999.9999 -> 99999)
            tempValue = Math.Floor(tempValue / 10000);
            unitIndex++;
        }

        long highValue = (long)tempValue;
        long lowValue = (long)lowValueDouble;

        StringBuilder sb = new StringBuilder();
        sb.Append($"{highValue}{Units[unitIndex]}");

        // 하위 단위가 0보다 크면 줄바꿈하여 표시
        if (lowValue > 0)
        {
            sb.Append($"\n{lowValue}{Units[unitIndex - 1]}");
        }

        return sb.ToString();
    }
}
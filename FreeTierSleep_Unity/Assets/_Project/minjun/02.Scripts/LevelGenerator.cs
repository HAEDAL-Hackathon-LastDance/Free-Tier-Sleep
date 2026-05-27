// 2. 강제 지그재그(Zig-Zag) 패턴 적용
float nextX;
if (lastPlatformPos.x < 0)
{
    // 이전 발판이 왼쪽이면 다음은 오른쪽으로
    nextX = Random.Range(2f, maxXClamp);
}
else
{
    // 이전 발판이 오른쪽(또는 중앙)이면 다음은 왼쪽으로
    nextX = Random.Range(minXClamp, -2f);
}

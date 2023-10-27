using System.Numerics;
public class Data
{
    private const ulong u = 1;
    public ulong White { get; set; }
    public ulong Black { get; set; }

    public Data(ulong white, ulong black)
    {
        White = white;
        Black = black;
    }

    public Data()
    {
        White = (u << 27) + (u << 36);
        Black = (u << 28) + (u << 35);
    }
}
public class Othello
{
    private const ulong u = 1;
    protected readonly int[] surrounding = new int[8] { 1, -1, 8, -8, 9, -9, 7, -7 };
    public Data data;
    public int whiteCount { get; set; } = 2;
    public int blackCount { get; set; } = 2;
    public bool white { get; set; } = false;
    public Othello(Data data, int white, int black, bool isWhite)
    {
        this.data = data;
        whiteCount = white;
        blackCount = black;
        this.white = isWhite;
    }
    public void Play(int position)
    {
        foreach (var s in surrounding)
        {
            var moveChange = MoveAndChange(s, position);

            data = new Data(moveChange.Item1.White, moveChange.Item1.Black);
            if (white)
            {
                whiteCount += moveChange.Item2;
                blackCount -= moveChange.Item2;
                continue;
            }

            whiteCount -= moveChange.Item2;
            blackCount += moveChange.Item2;
        }

        if (white)
        {
            data.White += u << position;
            whiteCount++;
            return;
        }

        data.Black += u << position;
        blackCount++;
    }
    public bool GameEnded()
    {
        if (
            (data.White | data.Black) == ulong.MaxValue ||
            data.White == 0 ||
            data.Black == 0
        )
            return true;

        var ended = true;

        for (int i = 63; i <= 0; i--)
        {
            if (CanPlay(i))
            {
                ended = false;
                break;
            }
        }

        return ended;
    }
    public (Data, int) MoveAndChange(int lineMovement, int currentPosition)
    {
        var (enemy, ally) = white ? (data.Black, data.White) : (data.White, data.Black);
        var updatedPosition = currentPosition + lineMovement;
        var positionIsEnemy = (enemy >>> (63 - updatedPosition) & 1) == 1;
        var positionIsAlly = (ally >>> (63 - updatedPosition) & 1) == 1;
        ulong mapChange = 0;

        while ((positionIsEnemy || positionIsAlly) && IsInArea(lineMovement, updatedPosition, currentPosition))
        {
            if (!positionIsEnemy && positionIsAlly)
            {
                var newDataWhite = white ? data.White + mapChange : data.White - mapChange;
                var newDataBlack = white ? data.Black - mapChange : data.Black + mapChange;
                var newData = new Data(newDataWhite, newDataBlack);

                var absDirection = Math.Abs(lineMovement);
                var distanceTraveled = Math.Abs(currentPosition - updatedPosition) - absDirection;
                var blocksTraveled = distanceTraveled / absDirection;

                return (newData, blocksTraveled);
            }

            if (!positionIsAlly && !positionIsEnemy)    
                return (data, 0);        

            mapChange += u << updatedPosition;
            updatedPosition += lineMovement;
            positionIsEnemy = (enemy >>> (63 - updatedPosition) & 1) == 1;
            // positionIsEnemy = ((enemy >>> updatedPosition) & 1) == 1;
            positionIsAlly = (ally >>> (63 - updatedPosition) & 1) == 1;
            // positionIsAlly = ((ally >>> updatedPosition) & 1) == 1;
        }

        return (data, 0);
    }
    public bool MoveIsFlanking(int currentPosition, int lineMovement)
    {
        var (enemy, ally) = white ? (data.Black, data.White) : (data.White, data.Black);
        var updatedPosition = lineMovement + currentPosition;
        var positionIsEnemy = ((enemy >>> updatedPosition) & 1) == 1;
        bool positionIsAlly;

        while (positionIsEnemy && IsInArea(lineMovement, updatedPosition, currentPosition))
        {
            updatedPosition += lineMovement;
            positionIsEnemy = ((enemy >>> updatedPosition) & 1) == 1;
            positionIsAlly = ((ally >>> updatedPosition) & 1) == 1;

            if (!positionIsEnemy && positionIsAlly)
                return true;

            if (!positionIsEnemy && !positionIsAlly)
                return false;
        }

        return false;
    }
    public bool IsInArea(int lineMovement, int currentPosition, int originalPosition)
    {
        var line = originalPosition % 8;
        var column = originalPosition / 8;
        var right = lineMovement == 7 || lineMovement == 9;

        if (currentPosition < 0 || currentPosition > 63)
            return false;

        switch (Math.Abs(lineMovement))
        {
            case 1:
                return currentPosition / 8 == column;

            case 8:
                return currentPosition % 8 == line;

            case 9:
                if (right)
                    return currentPosition % 8 > line && currentPosition % 9 == originalPosition % 9;
                return currentPosition % 8 < line && currentPosition % 9 == originalPosition % 9;

            case 7:
                if (right)
                    return currentPosition % 8 < line && currentPosition % 7 == originalPosition % 7;
                return currentPosition % 8 > line && currentPosition % 7 == originalPosition % 7;

            default:
                throw new Exception("Moio");
        }
    }
    public bool CanPlay(int position)
    {
        var enemy = white ? data.Black : data.White;

        var hasAdjacent = false;

        for (int i = 0; i < surrounding.Length; i++)
        {
            var newPosition = position + surrounding[i];

            if (newPosition < 0 || newPosition > 63)
                continue;

            if (
                position % 8 == 7 && (surrounding[i] == 1 || surrounding[i] == -7 || surrounding[i] == 9) ||
                position % 8 == 0 && (surrounding[i] == -1 || surrounding[i] == 7 || surrounding[i] == -9)
            )
                continue;

            if (((enemy >>> newPosition) & 1) == 1)
            {
                hasAdjacent = true;
                break;
            }
        }

        if (!hasAdjacent) return false;

        for (int i = 0; i < surrounding.Length; i++)
        {
            if (MoveIsFlanking(position, surrounding[i]))
                return true;
        }

        return false;
    }
    public Othello Clone()
    {
        Othello copy = new Othello(data, whiteCount, blackCount, white);
        return copy;
    }
    public IEnumerable<Othello> Next()
    {
        var clone = Clone();

        var gameMap = data.White | data.Black;

        for (int i = 63; i >= 0; i--)
        {
            var freePosition = ((gameMap >>> i) & 1) != 1;

            if (!freePosition)
                continue;

            if (!CanPlay(i))
                continue;

            clone.Play(i);
            clone.white = !white;
            yield return clone;
            clone = Clone();
        }
    }
}
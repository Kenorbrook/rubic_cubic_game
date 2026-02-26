public class GameSaves
{
    public int Character;

    public GameSaves(int character)
    {
        Character = character < 0 ? 0 : character;
    }

    public void ChangeCharacter(int character)
    {
        Character = character < 0 ? 0 : character;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class NameScript : MonoBehaviour
{
    public Text nameText;
    public float speed = 5f; // Speed of the movement
    private Vector2 targetPosition; // The target position to move towards
    private float xMax; // Max horizontal position on the screen
    private float yMax; // Max vertical position on the screen

    void Start()
    {
        targetPosition = transform.position; // Initialize target position
        xMax = Screen.width / 2; // Replace with actual Canvas size if different
        yMax = Screen.height / 2; // Replace with actual Canvas size if different
    }

    void Update()
    {
        // Lerp towards the target position each frame
        transform.position = Vector2.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
    }

    public void SetName(string name)
    {
        nameText.text = name;
    }

    public string GetName()
    {
        return nameText.text;
    }

    public void UpdateXY(float x, float y)
    {
        // Update the target position with the new x and y values
        targetPosition += new Vector2(x, y); // Adjust the target by the input values
    }
}
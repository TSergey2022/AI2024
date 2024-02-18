using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGrid : MonoBehaviour
{
  //  Модель для отрисовки узла сетки
  public GameObject nodeModel;

  //  Ландшафт (Terrain) на котором строится путь
  [SerializeField] private Terrain landscape = null;

  //  Шаг сетки (по x и z) для построения точек
  [SerializeField] private int gridDelta = 20;

  //  Номер кадра, на котором будет выполнено обновление путей
  private int updateAtFrame = 0;  

  //  Массив узлов - создаётся один раз, при первом вызове скрипта
  private PathNode[,] grid = null;

  // Метод вызывается однократно перед отрисовкой первого кадра
  void Start()
  {
    //  Создаём сетку узлов для навигации - адаптивную, под размер ландшафта
    Vector3 terrainSize = landscape.terrainData.bounds.size;
    int sizeX = (int)(terrainSize.x / gridDelta);
    int sizeZ = (int)(terrainSize.z / gridDelta);
    //  Создаём и заполняем сетку вершин, приподнимая на 25 единиц над ландшафтом
    grid = new PathNode[sizeX,sizeZ];
    for (int x = 0; x < sizeX; ++x)
      for (int z = 0; z < sizeZ; ++z)
      {
        Vector3 position = new Vector3(x * gridDelta, 0, z * gridDelta);
        position.y = landscape.SampleHeight(position) + 25;
        grid[x, z] = new PathNode(nodeModel, false, position);
        grid[x, z].ParentNode = null;
        grid[x, z].Fade();
      }
  }

  private Amogus amogus = new Amogus();

  [SerializeField] [Range(0, 10000)] float val = 40;

  void Update() {
    amogus.ProcessInput();
    if (Time.frameCount < updateAtFrame || amogus.Lock) return;
    // Debug.Log("Amogus");
    // updateAtFrame = Time.frameCount + 60;
    amogus.val = val;
    amogus.Process(grid, Vector2Int.zero, new Vector2Int(grid.GetUpperBound(0), grid.GetUpperBound(1)));
  }

  void OnGUI() {
    amogus.ProcessGui();
  }

}

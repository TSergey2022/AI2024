using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Utils;

public class Amogus {

  public bool Lock { get; private set; } = false;

  public float val = 40;

  private bool procWave = true;
  private bool procDijkstra = true;
  private bool procAStar = true;

  public float Dist(PathNode a, PathNode b) {
    Vector3 aa = a.pos, bb = b.pos;
    return Vector3.Distance(aa, bb) + val * Mathf.Abs(aa.y - bb.y);
  }

  private List<Vector2Int> GetNeighbours(PathNode[,] grid, Vector2Int current) {
    List<Vector2Int> nodes = new List<Vector2Int>();
    for (int x = current.x - 1; x <= current.x + 1; ++x)
      for (int y = current.y - 1; y <= current.y + 1; ++y)
        if (x >= 0 && y >= 0 && x < grid.GetLength(0) && y < grid.GetLength(1) && (x != current.x || y != current.y))
          nodes.Add(new Vector2Int(x, y));
    return nodes;
  }

  private void UpdatePos(PathNode[,] grid) {
    foreach (PathNode node in grid) {
      node.pos = node.body.transform.position;
    }
  }

  private void ResetNodes(PathNode[,] grid) {
    foreach (PathNode node in grid) {
      node.ParentNode = null;
      node.Distance = float.PositiveInfinity;
    }
  }

  private void UpdateWalkableNodes(PathNode[,] grid) {
    foreach (PathNode node in grid) {
      node.walkable = !Physics.CheckSphere(node.body.transform.position, 1);
    }
  }

  private List<PathNode> ProcessWave(PathNode[,] grid, Vector2Int startNode, Vector2Int finishNode) {
    ResetNodes(grid);
    PathNode start = grid[startNode.x, startNode.y];
    start.Distance = 0;
    Queue<Vector2Int> nodes = new Queue<Vector2Int>();
    nodes.Enqueue(startNode);
    while(nodes.Count != 0) {
      Vector2Int current = nodes.Dequeue();
      if (current == finishNode) break;
      var neighbours = GetNeighbours(grid, current);
      foreach (var node in neighbours) {
        var c = grid[current.x, current.y];
        var n = grid[node.x, node.y];
        if (!n.walkable) continue;
        if (n.Distance > c.Distance + Dist(n, c)) {
          n.Distance = c.Distance + Dist(n, c);
          n.ParentNode = c;
          nodes.Enqueue(node);
        }
      }
    }
    var rez = new List<PathNode>();
    var pathElem = grid[finishNode.x, finishNode.y];
    while(pathElem != null) {
      rez.Add(pathElem);
      pathElem = pathElem.ParentNode;
    }
    rez.Reverse();
    return rez;
  }

  private List<PathNode> ProcessDijkstra(PathNode[,] grid, Vector2Int startNode, Vector2Int finishNode) {
    ResetNodes(grid);
    PathNode start = grid[startNode.x, startNode.y];
    start.Distance = 0;

    var queue = new PriorityQueue<Vector2Int, float>();
    queue.Enqueue(startNode, 0);
    while (queue.Count > 0) {
      var current = queue.Dequeue();
      if (current == finishNode) break;
      var neighbours = GetNeighbours(grid, current);
      foreach (var node in neighbours) {
        var c = grid[current.x, current.y];
        var n = grid[node.x, node.y];
        if (!n.walkable) continue;
        if (n.Distance > c.Distance + Dist(n, c)) {
          n.Distance = c.Distance + Dist(n, c);
          n.ParentNode = c;
          queue.Enqueue(node, n.Distance);
        }
      }
    }
    var rez = new List<PathNode>();
    var pathElem = grid[finishNode.x, finishNode.y];
    while (pathElem != null) {
      rez.Add(pathElem);
      pathElem = pathElem.ParentNode;
    }
    rez.Reverse();
    return rez;
  }

  public List<PathNode> ProcessAStar(PathNode[,] grid, Vector2Int startNode, Vector2Int finishNode) {
    ResetNodes(grid);
    PathNode start = grid[startNode.x, startNode.y];
    start.Distance = 0;

    var queue = new PriorityQueue<Vector2Int, float>();
    queue.Enqueue(startNode, 0);
    while (queue.Count > 0) {
      var current = queue.Dequeue();
      if (current == finishNode) break;
      var neighbours = GetNeighbours(grid, current);
      foreach (var node in neighbours) {
        var c = grid[current.x, current.y];
        var n = grid[node.x, node.y];
        var f = grid[finishNode.x, finishNode.y];
        if (!n.walkable) continue;
        if (n.Distance > c.Distance + Dist(c, n)) {
          n.Distance = c.Distance + Dist(c, n);
          n.ParentNode = c;
          queue.Enqueue(node, n.Distance + Dist(n, f));
        }
      }
    }

    var rez = new List<PathNode>();
    var pathElem = grid[finishNode.x, finishNode.y];
    while (pathElem != null) {
      rez.Add(pathElem);
      pathElem = pathElem.ParentNode;
    }
    rez.Add(grid[startNode.x, startNode.y]);
    rez.Reverse();
    return rez;
  }

  async public void Process(PathNode[,] grid, Vector2Int startNode, Vector2Int finishNode) {
    // if (!(procWave || procDijkstra || procAStar)) return;
    if (Lock) return;
    Lock = true;
    UpdatePos(grid);
    UpdateWalkableNodes(grid);
    var (wave, dijkstra, astar) = await Task.Run(() => {
      var wave = procWave ? ProcessWave(grid, startNode, finishNode) : new List<PathNode>();
      var dijkstra = procDijkstra ? ProcessDijkstra(grid, startNode, finishNode) : new List<PathNode>();
      var astar = procAStar ? ProcessAStar(grid, startNode, finishNode) : new List<PathNode>();
      return (wave, dijkstra, astar);
    });
    foreach (var node in grid)
      if (node.walkable) node?.Fade();
      else node?.Illuminate();
    if (wave.Count > 0) wave.ForEach((node)=>node?.Illuminate2());
    if (dijkstra.Count > 0) dijkstra.ForEach((node)=>node?.Illuminate3());
    if (astar.Count > 0) astar.ForEach((node)=>node?.Illuminate4());
    Lock = false;
  }

  public void ProcessInput() {
    if (Input.GetKeyDown(KeyCode.Alpha1)) procWave = !procWave;
    if (Input.GetKeyDown(KeyCode.Alpha2)) procDijkstra = !procDijkstra;
    if (Input.GetKeyDown(KeyCode.Alpha3)) procAStar = !procAStar;
  }

  public void ProcessGui(int cock) {
    GUI.backgroundColor = Color.black;
    GUIStyle style = new GUIStyle(GUI.skin.box);
    style.active = style.normal;
    style.fontSize = 20;
    style.normal.textColor = procWave ? Color.yellow : Color.red;
    GUI.Box(new Rect(10, 10, 200, 30), $"Wave is {(procWave ? "active" : "deactive")}", style);
    style.normal.textColor = procDijkstra ? Color.cyan : Color.red;
    GUI.Box(new Rect(10, 40, 200, 30), $"Dijkstra is {(procDijkstra ? "active" : "deactive")}", style);
    style.normal.textColor = procAStar ? Color.green : Color.red;
    GUI.Box(new Rect(10, 70, 200, 30), $"A* is {(procAStar ? "active" : "deactive")}", style);
    style.normal.textColor = Color.magenta;
    GUI.Label(new Rect(10, 100, 200, 30), $"{cock}", style);
  }

}
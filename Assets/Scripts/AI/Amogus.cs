using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Utils;

public class Amogus {

  public bool Lock { get; private set; } = false;

  public float Dist(PathNode a, PathNode b) {
    return Vector3.Distance(a.pos, b.pos) + 40 * Mathf.Abs(a.pos.y - b.pos.y);
  }

  private List<Vector2Int> GetNeighbours(PathNode[,] grid, Vector2Int current) {
    List<Vector2Int> nodes = new List<Vector2Int>();
    for (int x = current.x - 1; x <= current.x + 1; ++x)
      for (int y = current.y - 1; y <= current.y + 1; ++y)
        if (x >= 0 && y >= 0 && x < grid.GetLength(0) && y < grid.GetLength(1) && (x != current.x || y != current.y))
          nodes.Add(new Vector2Int(x, y));
    return nodes;
  }

  private void ResetNodes(PathNode[,] grid) {
    foreach (PathNode node in grid) {
      node.ParentNode = null;
      node.Distance = float.PositiveInfinity;
      node.pos = node.body.transform.position;
    }
  }

  private void UpdateWalkableNodes(PathNode[,] grid) {
    foreach (PathNode node in grid) {
      node.walkable = !Physics.CheckSphere(node.body.transform.position, 1);
    }
  }

  private List<PathNode> ProcessWave(PathNode[,] grid, Vector2Int startNode, Vector2Int finishNode) {
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
        if (n.walkable && n.Distance > c.Distance + Dist(n, c)) {
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
        if (n.walkable && n.Distance > c.Distance + Dist(n, c)) {
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
    var priorityQueue = new PriorityQueue<Vector2Int, float>();
    var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
    var costSoFar = new Dictionary<Vector2Int, float>();

    priorityQueue.Enqueue(startNode, 0);
    cameFrom[startNode] = startNode;
    costSoFar[startNode] = 0;

    while (priorityQueue.Count > 0) {
      var current = priorityQueue.Dequeue();
      if (current == finishNode) break;
      var neighbours = GetNeighbours(grid, current);
      foreach (var node in neighbours) {
        var c = grid[current.x, current.y];
        var n = grid[node.x, node.y];
        var f = grid[finishNode.x, finishNode.y];
        float newCost = costSoFar[current] + Dist(c, n);
        if (!costSoFar.ContainsKey(node) || newCost < costSoFar[node]) {
          costSoFar[node] = newCost;
          float priority = newCost + Dist(n, f);
          priorityQueue.Enqueue(node, priority);
          cameFrom[node] = current;
        }
      }
    }

    var rez = new List<PathNode>();
    var pathElem = finishNode;
    while (pathElem != startNode) {
      rez.Add(grid[pathElem.x, pathElem.y]);
      pathElem = cameFrom[pathElem];
    }
    rez.Add(grid[startNode.x, startNode.y]);
    rez.Reverse();
    return rez;
  }

  async public void Process(PathNode[,] grid, Vector2Int startNode, Vector2Int finishNode) {
    if (Lock) return;
    Lock = true;
    ResetNodes(grid);
    UpdateWalkableNodes(grid);
    var wave = await Task.Run(()=>ProcessWave(grid, startNode, finishNode));
    var dijkstra = await Task.Run(()=>ProcessDijkstra(grid, startNode, finishNode));
    var astar = await Task.Run(()=>ProcessAStar(grid, startNode, finishNode));
    if (grid[0, 0] == null) return;
    foreach (var node in grid)
      if (node.walkable) node.Fade();
      else node.Illuminate();
    wave.ForEach((node)=>node.Illuminate2());
    dijkstra.ForEach((node)=>node.Illuminate3());
    astar.ForEach((node)=>node.Illuminate4());
    Lock = false;
  }

}
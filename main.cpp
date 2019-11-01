#include <iostream>
#include <vector>
#include <iomanip>
#include <cassert>

#include <boost/algorithm/string.hpp>


class Spieler {
    std::string names[4];
public:
    Spieler(std::string A, std::string B, std::string C, std::string D) {
        names[0] = A;
        names[1] = B;
        names[2] = C;
        names[3] = D;
    }

    int getPlayer(std::string player) {
        for(int i = 0; i < 4; i++) {
            if (names[i] == player) return i;

        }
        throw std::exception();
    }

    std::string getName(int idx) {
        return names[idx];
    }

    int getSpace(int idx) {
        int l = names[idx].length();
        if (l < 5) return 5;
        return l;
    }

    int getAllSpace() {
        int sum = 0;
        for(int i = 0; i < 4; i++) {
            sum+=getSpace(i);
        }
        return sum;
    }
};

class State {

    int player[4];
public:
    State() {
        for (int i = 0; i < 4; i++) {
            player[i] = 0;
        }
    }

    State(int p1, int p2, int p3, int p4) {
        assert( (p1+p2+p3+p4) == 0);

        player[0] = p1;
        player[1] = p2;
        player[2] = p3;
        player[3] = p4;
    }

    State operator+(const State & rhs) const {

        return State(
                player[0] + rhs.player[0],
                player[1] + rhs.player[1],
                player[2] + rhs.player[2],
                player[3] + rhs.player[3]);

    }

    void print(Spieler *spieler) {
        for(int i = 0; i < 4; i++) {
            std::cout << " | " << std::setw(spieler->getSpace(i)) << player[i];
        }
        std::cout << " |";
    }
};

class Game {
    std::vector<State> perGamePoints;
    std::vector<int> value;
    std::vector<State> GamePoints;

    void newGame(int d1, int d2, int d3, int d4) {
        perGamePoints.push_back(State(d1,d2,d3,d4));
        if (GamePoints.empty()) {
            GamePoints.push_back(perGamePoints.back());
        } else {
            GamePoints.push_back(GamePoints.back() + perGamePoints.back());
        }
    }

    Spieler *spieler;

public:

    Game(Spieler & spieler) : spieler(&spieler){
    }

    void loseSolo(int player, int points) {
        winSolo(player, -points);
    }

    void winSolo(int player, int points)  {
        assert(player >= 0);
        assert(player < 4);

        value.push_back(points);

        int point[4];

        for(int i = 0; i < 4; i++) {
            point[i] = -points;
        }

        point[player] = 3 * points;

        newGame(point[0], point[1], point[2], point[3]);
    }

    void normal(int player1, int player2, int points) {
        assert(player1 >= 0);
        assert(player1 < 4);
        assert(player2 >= 0);
        assert(player2 < 4);
        assert(player1 != player2);

        value.push_back(points);
        int point[4];

        for(int i = 0; i < 4; i++) {
            point[i] = -points;
        }

        point[player1] = points;
        point[player2] = points;

        newGame(point[0], point[1], point[2], point[3]);
    }

    void printHeader() {
        int i;
        for(i = 0; i < 4; i++) {
            std::cout << " | " << std::setw(spieler->getSpace(i)) << spieler->getName(i);
        }

        std::cout << " | " << std::setw(5) << "Spiel" << " |" << std::endl << " ";
        for(i = 0; i < 4*4 + 5 + spieler->getAllSpace();i++) {
            std::cout << "-";
        }
        std::cout << std::endl;
    }

    void printHistory() {
        printHeader();

        for(int i = 0; i < GamePoints.size(); i++) {
            GamePoints[i].print(spieler);

            std::cout << std::setw(5) << value[i] << " | " << std::endl;

        }
    }

    void printState() {
        printHeader();
        GamePoints.back().print(spieler);
        std::cout << std::setw(5) << value.back() << " | " << std::endl;
    }


};

int main(int argc, char ** argv) {
    std::string names[4];
    if ( argc < 5) {
        names[0] = "Johannes";
        names[1] = "Alex";
        names[2] = "Sinan";
        names[3] = "Hannes";
    } else {
        for ( int i = 0; i< 4; i++) {
            names[i] = argv[i+1];
        }
    }
    Spieler spieler(names[0], names[1], names[2], names[3]);
    Game game(spieler);

    std::string input;

    while (true) {
        std::cout << ">> ";
        std::getline(std::cin, input);
        if (input == "spiel") {
            game.printHistory();
        } else if (input == "END") {
            break;
        } else {
            //krumme dinge
            int ce = input.find(" ");
            std::vector<std::string> token;
            boost::split(token, input, boost::is_any_of(" "));
            std::string cmd = token[0];
            try {
                if (cmd == "s" || cmd == "solo" || cmd == "solor" || cmd == "sr" || cmd == "solore") {
                    if (token.size() == 3) {
                        game.winSolo(spieler.getPlayer(token[1]), std::stoi(token[2]));
                        game.printState();
                    }
                } else if (cmd == "sc" || cmd == "soloc" || cmd == "sk" || cmd == "solok" ||
                           cmd == "solocontra" || cmd == "solokontra") {
                    if (token.size() == 3) {
                        game.loseSolo(spieler.getPlayer(token[1]), std::stoi(token[2]));
                        game.printState();
                    }
                } else if (cmd == "n" || cmd == "normal" || cmd == "g" || cmd == "gesund") {
                    if (token.size() == 4) {
                        game.normal(spieler.getPlayer(token[1]) , spieler.getPlayer(token[2]), std::stoi(token[3]));
                        game.printState();
                    }
                } else {
                    std::cout << "unbekanntes Befehl: " << cmd << std::endl;
                }
            } catch(std::exception &e) {
                std::cout << "Spieler nicht bekannt" << std::endl;
            }
        }
    }

    std::cout << "Tschüüüs" << std::endl;
    return 0;
}
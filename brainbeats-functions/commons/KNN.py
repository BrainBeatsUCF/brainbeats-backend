from math import sqrt
from collections import defaultdict

class KNN:
    def __init__(self, K: int):
        self.K = K

    def get_distance(self, src: dict, dest: dict) -> float:
        """
        Parameters
        ----------
        src : dict
            Weighted dimensions of the source point
        dest : set
            Properties of the destination point

        Returns
        -------
        float
            The n-dimensional Euclidean distance between src and dest
        """

        dist_sum = 0
        for key in src.keys():
            src_val = src[key]
            dest_val = 0

            if key in dest:
                dest_val = 1

            dist_sum += pow(src_val - dest_val, 2)

        return sqrt(dist_sum)

    def get_beat_dimensions(self, item: dict) -> set:
        """
        Parameters
        ----------
        item : dict
            A Gremlin Beat object

        Returns
        -------
        set
            A set of dimensions representing the Beat object

        """

        instruments = set()
        genres = set()

        if 'instrumentList' in item['properties']:
            instruments = set(item['properties']['instrumentList'][0]['value'].split(','))

        if 'genre' in item['properties']:
            genres = set(item['properties']['genre'][0]['value'].split(','))

        return instruments | genres

    def get_user_query_weights(self, query: list) -> dict:
        """
        Parameters
        ----------
        query : list
            List of Gremlin Beat objects

        Returns
        -------
        dict
            A dictionary representing the weighted dimensional values of the
            query object
        """

        dimension_map = defaultdict(int)
        total_dimensions = set()
        weighted_dimensions = dict()

        # Get all unique dimensions
        for item in query:
            for dimension in self.get_beat_dimensions(item):
                dimension_map[dimension] = []

        # For each Beat, assign a dimensional value of '1' if that Beat corresponds
        # to that specific dimension
        for item in query:
            query_dimensions = self.get_beat_dimensions(item)

            for dimension in dimension_map:
                if dimension in query_dimensions:
                    dimension_map[dimension].append(1)
                else:
                    dimension_map[dimension].append(0)

        # Get the weights for each dimension
        for dimension in dimension_map:
            weighted_dimensions[dimension] = sum(dimension_map[dimension]) / len(dimension_map[dimension])

        return weighted_dimensions

    def get_k_nearest_neighbors(self, user_likes_dataset: list, full_dataset: list) -> list:
        """
        Parameters
        ----------
        user_likes_dataset : list
            List of Gremlin Beat objects representing the user's listening preferences
        full_dataset : list
            List of Gremlin Beat objects representing the full collection of Beats to get
            the K nearest neighbors from

        Returns
        -------
        list
            List of Gremlin Beat objects representing the K Nearest Neighbors
        """

        query_dimensions = self.get_user_query_weights(user_likes_dataset)

        distances = []
        for item in full_dataset:
            item_dimensions = self.get_beat_dimensions(item)
            distances.append((item, self.get_distance(query_dimensions, item_dimensions)))

        # Sort the list of distances by the 2nd value in the tuple
        distances = sorted(distances, key = lambda x: x[1])
        # Get the K closest points
        distances = distances[:self.K]

        nearest_beats = []
        for item in distances:
            nearest_beats.append(item[0])

        return nearest_beats
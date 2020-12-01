from gremlin_python.driver import client, serializer
from typing import Coroutine

class DatabaseConnection:
    def __init__(self, url, database_name, collection_name, key):
        self.gremlin_client = client.Client(url,'g', 
            username = f'/dbs/{database_name}/colls/{collection_name}', 
            password = key,
            message_serializer = serializer.GraphSONSerializersV2d0())

        self.query_map = {
            'get_all_beats': 'g.V().hasLabel(\'beat\')',
            'get_all_samples': 'g.V().hasLabel(\'sample\')',
            'get_all_users': 'g.V().hasLabel(\'user\')',
            'recommend_beat': 'g.V(\'{email}\').property(\'propertyType\', \'{current_time}\').addE(\'RECOMMENDED\').to(g.V(\'{beat_id}\')).property(\'propertyType\', \'{current_time}\')',
            'get_all_public_beats': 'g.V().hasLabel(\'beat\').has(\'isPrivate\', \'False\')',
            'get_liked_beats': 'g.V(\'{email}\').outE(\'LIKES\').order().by(\'date\', decr).inV().limit({limit}).hasLabel(\'beat\')',
            'delete_recommendations': 'g.V(\'{email}\').outE(\'RECOMMENDED\').drop()'
        }

    def execute_query(self, query_string: str) -> Coroutine[None, None, list]:
        callback = self.gremlin_client.submitAsync(query_string)

        results = list()

        if callback is None:
            return results

        [results.extend(result) for result in callback.result()]

        return results

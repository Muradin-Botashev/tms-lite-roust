import React from 'react';
import { useSelector } from 'react-redux';
import { historySelector, progressSelector } from '../../../../ducks/history';
import { Dimmer, Grid, Loader } from 'semantic-ui-react';
import { dateToUTC } from '../../../../utils/dateTimeFormater';

const History = () => {
    const history = useSelector(state => historySelector(state));

    const loading = useSelector(state => progressSelector(state));

    return (
        <div className="tabs-card tabs-card_history">
            <Grid>
              {/*  <Dimmer active={loading} inverted>
                    <Loader size="huge">Loading</Loader>
                </Dimmer>*/}
                {(history || []).map((historyItem, i) => (
                    <Grid.Row key={i}>
                        <Grid.Column width={5}>
                            <div>{dateToUTC(historyItem.createdAt, 'DD.MM.YYYY HH:mm')}</div>
                            <div className="history-who">{historyItem.userName}</div>
                        </Grid.Column>
                        <Grid.Column width={11}>
                            <div className="history-comment"> {historyItem.message} </div>
                        </Grid.Column>
                    </Grid.Row>
                ))}
            </Grid>
        </div>
    );
};

export default History;

import React, { useState, useEffect, useRef } from 'react';
import { Checkbox, Dimmer, Form, Input, Loader, Visibility } from 'semantic-ui-react';

import { useSelector, useDispatch } from 'react-redux';
import { clearLookup, getLookupRequest, listSelector, progressSelector } from '../../ducks/lookup';
import { PAGE_SIZE } from '../../constants/settings';
import Search from '../Search';

const Facet = ({t, source, name, onChange, value, gridName, filters = {}}) => {
    const dispatch = useDispatch();

    let [filter, setFilter] = useState(null);
    let [items, setItems] = useState([]);
    let [counter, setCounter] = useState(PAGE_SIZE);

    const valuesList = useSelector(state => listSelector(state, filter, t));
    const loading = useSelector(state => progressSelector(state));

    const context = useRef(null);

    const inputRef = useRef(null);

    useEffect(() => {
        setTimeout(() => {
            inputRef && inputRef.current && inputRef.current.focus();
        }, 500)
    }, []);

    useEffect(() => {
        handleOpen();
        return clearFilter;
    }, []);

    useEffect(
        () => {
            changeItems();
        },
        [valuesList, counter],
    );

    useEffect(
        () => {
            context.current.scrollTop = 0;
            setCounter(PAGE_SIZE);
        },
        [filter],
    );

    const changeItems = () => {
        setItems(valuesList.slice(0, counter));
    };

    const handleSetFilter = (e, { value }) => {
        setFilter(value);
    };

    const clearFilter = () => {
        setFilter(null);
        dispatch(clearLookup());
    };

    const handleOpen = () => {
        dispatch(
            getLookupRequest({
                name,
                entities: gridName,
                isFilter: true,
                params: filters.filter,
            }),
        );
    };

    const handleRestClick = () => {
        if (onChange !== undefined) onChange(null, { name: name, value: null });
    };

    const toggle = (e, { value: newValue }) => {
        let values = value ? value.split('|') : [];

        if (values.some(x => x === newValue)) {
            values.splice(values.indexOf(newValue), 1);
        } else {
            values.push(newValue);
        }
        if (onChange !== undefined) onChange(e, { name: name, value: values.join('|') });
    };

    const scroll = () => {
        if (counter < valuesList.length) {
            setCounter(prevState => prevState + PAGE_SIZE);
        }
    };

    let values = value ? value.split('|') : [];

    return (
        <div className="facet-input">
            <Form>
                {/*<label className="label-in-popup">{t(name)}</label>*/}
                <div>
                    <Search
                        ref={inputRef}
                        fluid
                        size="mini"
                        placeholder=""
                        isAuto
                        onChange={handleSetFilter}
                    />
                    {/* <Input
                        fluid
                        size="mini"
                        icon="search"
                        value={filter}
                        onChange={handleSetFilter}
                    />*/}
                </div>
                <div className="reset-selected">
                    <span onClick={handleRestClick}>{t('reset_selected')}</span>
                </div>
                <div className="select-facet-values" ref={context}>
                    <Dimmer active={loading} inverted>
                        <Loader size="small">Loading</Loader>
                    </Dimmer>
                    <div style={{ position: 'relative' }}>
                        {items &&
                            items.map(x => {
                                let label = <label>{x.name}</label>;
                                return (
                                    <Form.Field
                                        key={x.value}
                                        className={!x.isActive ? 'colorGrey' : ''}
                                    >
                                        <Checkbox
                                            value={x.value}
                                            checked={values.includes(x.value)}
                                            onChange={toggle}
                                            label={label}
                                        />
                                    </Form.Field>
                                );
                            })}
                        <Visibility
                            continuous={true}
                            once={false}
                            context={context.current}
                            onTopVisible={scroll}
                            style={{
                                position: 'absolute',
                                bottom: '20px',
                                left: 0,
                                right: 0,
                                zIndex: -1,
                            }}
                        />
                    </div>
                </div>
            </Form>
        </div>
    );
};

export default Facet;
